using System.Collections.Generic;
using System.IO;
using System.Text;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.File
{
    /// <summary>
    /// A basic implementation of the <see cref="IParser"/> interface.
    /// </summary>
    /// <typeparam name="TValue">The record type.</typeparam>
    /// <typeparam name="TSerializer"></typeparam>
    internal abstract class BinaryFileParser<TValue, TSerializer> : BinaryReader, IParser<TValue>
        where TSerializer : ISerializer<TValue>, new()
    {
        protected TypeInfo Type { get; }

        /// <summary>
        /// The header.
        /// </summary>
        public abstract ref readonly IFileHeader Header { get; }

        /// <summary>
        /// The options to use for parsing.
        /// </summary>
        public ref readonly StorageOptions Options => ref _options;

        public TSerializer Serializer { get; }

        /// <summary>
        /// The first block of the file (typically its header).
        /// </summary>
        protected Block Head { get; set; }

        private StorageOptions _options;
        private BlockHandlers _handlers { get; } = new BlockHandlers();

        /// <summary>
        /// Create an instance of <see cref="BinaryFileParser{TValue, TSerializer}"/>.
        /// </summary>
        /// <param name="options">The options to use for parsing.</param>
        /// <param name="input">The input stream.</param>
        /// <param name="leaveOpen">If <c>true</c>, the stream is left open once this object is disposed.</param>
        protected BinaryFileParser(in StorageOptions options, Stream input) : base(input, Encoding.UTF8, true)
        {
            Type = new TypeInfo(typeof(TValue));
            _options = options;

            Serializer = new TSerializer();
            Serializer.Initialize(Type, in Options);
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
            var itr = Head;
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
        protected abstract void Prepare();

        public abstract BaseMemberMetadata GetFileMemberMetadata(int index);

        /// <summary>
        /// Obtains an instance of <see cref="IRecordReader"/> used for record reading.
        /// </summary>
        /// <returns></returns>
        protected abstract IRecordReader GetRecordReader();

        public virtual IEnumerable<TValue> Records
        {
            get
            {
                Prepare();

                var head = Head;
                while (head != null)
                {
                    _handlers.ReadBlock(this, head);
                    head = head.Next;
                }

                var copyTableHandler = FindBlockHandler<CopyTableHandler<int>>(BlockIdentifier.CopyTable);

                foreach (var instance in EnumerateRecordBlock())
                {
                    yield return instance;

                    if (copyTableHandler != null)
                    {
                        var instanceKey = Serializer.GetKey(in instance);

                        foreach (var cloneStore in copyTableHandler[instanceKey])
                        {
                            var clonedInstance = Serializer.Clone(instance);
                            Serializer.SetKey(out clonedInstance, cloneStore);
                            yield return clonedInstance;
                        }
                    }
                }
            }
        }

        private IEnumerable<TValue> EnumerateRecordBlock()
        {
            var recordBlock = FindBlock(BlockIdentifier.Records);
            if (recordBlock == null || recordBlock.Length == 0)
                yield break;

            BaseStream.Position = recordBlock.StartOffset;

            while (BaseStream.Position < recordBlock.EndOffset)
            {
                var instance = Serializer.Deserialize(GetRecordReader());
                yield return instance;
            }
        }

        public abstract int Size { get; }
    }
}
