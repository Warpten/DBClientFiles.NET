using System.Collections.Generic;
using System.IO;
using System.Text;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Types;

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

        public override sealed IEnumerable<Proxy<T>> Records
        {
            get
            {
                var copyTableHandler = FindBlockHandler<CopyTableHandler<TKey>>(BlockIdentifier.CopyTable);

                var proxyInstance = 0u;
                foreach (var record in base.Records)
                {
                    foreach (var cloneStore in copyTableHandler[KeySerializer.GetKey(record.Instance)])
                    {
                        var clonedInstance = Serializer.Clone(record.Instance);
                        KeySerializer.SetKey(clonedInstance, cloneStore);

                        yield return new Proxy<T>() {
                            Instance = clonedInstance,
                            UUID = proxyInstance
                        };

                        ++proxyInstance;
                    }

                    // Force update the proxy ID
                    record.UUID = proxyInstance;
                    yield return record;

                    ++proxyInstance;
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
        public BinaryFileReader(StorageOptions options, Stream input, bool leaveOpen) : base(input, Encoding.UTF8, leaveOpen)
        {
            Type = TypeInfo.Create<T>();
            Options = options;
            Head = new Block();

            RegisterBlockHandler(new StringBlockHandler(options.InternStrings));
        }

        protected override void Dispose(bool disposing)
        {
            Head = null;
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

        protected virtual IRecordReader GetRecordReader()
        {
            return new BaseRecordReader(this, Header.RecordSize, BaseStream);
        }

        public abstract ISerializer<T> Serializer { get; }

        public virtual IEnumerable<Proxy<T>> Records
        {
            get
            {
                Head.Identifier = BlockIdentifier.Header;
                Head.Length = Header.Size;

                Header.Read(this);
                PrepareBlocks();

                // Iterate the dolly chain of blocks and parse each of them given the prerecorded handlers
                Block head = Head;
                while (head != null)
                {
                    _handlers.ReadBlock<BinaryFileReader<T>, T>(this, head);
                    head = head.Next;
                }

                Block recordBlock = FindBlock(BlockIdentifier.Records);
                if (recordBlock != null && recordBlock.Exists)
                {
                    var proxyIndex = 0u;

                    BaseStream.Position = recordBlock.StartOffset;
                    while (BaseStream.Position < recordBlock.EndOffset)
                    {
                        using (var recordReader = GetRecordReader())
                        {
                            yield return new Proxy<T>() {
                                Instance = Serializer.Deserialize(recordReader),
                                UUID = proxyIndex
                            };
                        }

                        ++proxyIndex;
                    }
                }
            }
        }
    }
}
