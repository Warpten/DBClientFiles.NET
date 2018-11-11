using System.Collections.Generic;
using System.IO;
using System.Text;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization;

#if PROFILING_API
using JetBrains.Profiler.Windows.Api;
#endif

namespace DBClientFiles.NET.Parsing.File
{
    /// <summary>
    /// An abstract specialization of <see cref="BinaryFileReader{T}"/> for record types that have a key.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="T"></typeparam>
    internal abstract class BinaryFileReader<TKey, T> : BinaryFileReader<T>
    {
        public BinaryFileReader(StorageOptions options, Stream input, bool leaveOpen) : base(options, input, leaveOpen)
        {
            RegisterBlockHandler<CopyTableHandler<TKey>>();
        }

        public abstract ISerializer<TKey, T> KeySerializer { get; }
        public override ISerializer<T> Serializer => KeySerializer;

        public override sealed IEnumerable<T> Records
        {
            get
            {
                var copyTableHandler = FindBlockHandler<CopyTableHandler<TKey>>(BlockIdentifier.CopyTable);

                foreach (var record in base.Records)
                {
                    // TODO: Watch out not to cause promotion of base.Records's iterator to G2!
                    foreach (var cloneStore in copyTableHandler[KeySerializer.GetKey(record)])
                    {
                        var clonedInstance = Serializer.Clone(record);
                        KeySerializer.SetKey(clonedInstance, cloneStore);
                        yield return clonedInstance;
                    }

                    yield return record;
                }
            }
        }
    }

    /// <summary>
    /// A basic implementation of the <see cref="IReader{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    internal abstract class BinaryFileReader<T> : BinaryReader, IReader<T>
    {
        protected TypeInfo Type { get; }

        /// <summary>
        /// The header.
        /// </summary>
        public abstract IFileHeader Header { get; }

        /// <summary>
        /// The options to use for parsing.
        /// </summary>
        public StorageOptions Options { get; }

        /// <summary>
        /// The first block of the file (typically its header).
        /// </summary>
        protected Block Head { get; set; }

        private BlockHandlers _handlers { get; } = new BlockHandlers();

        /// <summary>
        /// Create an instance of <see cref="BinaryFileReader{T}"/>.
        /// </summary>
        /// <param name="options">The options to use for parsing.</param>
        /// <param name="input">The input stream.</param>
        /// <param name="leaveOpen">If <c>true</c>, the stream is left open once this object is disposed.</param>
        public BinaryFileReader(StorageOptions options, Stream input, bool leaveOpen) : base(input, Encoding.UTF8,
            leaveOpen)
        {
            Type = new TypeInfo(typeof(T));
            Options = options;

            RegisterBlockHandler(new StringBlockHandler(options.InternStrings));
        }

        public void Initialize()
        {
            Head = new Block {
                Identifier = BlockIdentifier.Header,
                Length = Header.Size + 4
            };
        }

        protected override void Dispose(bool disposing)
        {
            Head = default;
            _handlers.Clear();

            base.Dispose(disposing);
        }

        public Block FindBlock(BlockIdentifier identifier)
        {
            Block itr = Head;
            while (itr != null && itr.Identifier != identifier)
                itr = itr.Next;
            return itr;
        }

        public U FindBlockHandler<U>(BlockIdentifier identifier) where U : IBlockHandler
            => _handlers.GetHandler<U>(identifier);

        public void RegisterBlockHandler<U>() where U : IBlockHandler, new()
            => RegisterBlockHandler(new U());
        public void RegisterBlockHandler(IBlockHandler handler)
            => _handlers.Register(handler);

        /// <summary>
        /// Superclasses need implement this method to set up the linked chain of file blocks.
        /// </summary>
        protected abstract void PrepareBlocks();

        /// <summary>
        /// Obtains an instance of <see cref="IRecordReader"/> used for record reading.
        /// </summary>
        /// <returns></returns>
        protected abstract IRecordReader GetRecordReader();

        public abstract ISerializer<T> Serializer { get; }

        public virtual IEnumerable<T> Records
        {
            get
            {
#if PROFILING_API
                if (MemoryProfiler.IsActive && MemoryProfiler.CanControlAllocations)
                    MemoryProfiler.EnableAllocations();

                MemoryProfiler.Dump();
#endif

                PrepareBlocks();

                Block head = Head;
                while (head != null)
                {
                    _handlers.ReadBlock<BinaryFileReader<T>, T>(this, head);
                    head = head.Next;
                }

                var recordBlock = FindBlock(BlockIdentifier.Records);
                BaseStream.Position = recordBlock.StartOffset;

                while (BaseStream.Position < recordBlock.EndOffset)
                {
                    var instance = Serializer.Deserialize(GetRecordReader());
                    yield return instance;
                }

#if PROFILING_API
                MemoryProfiler.Dump();
#endif
            }
        }

        public abstract int Size { get; }
    }
}
