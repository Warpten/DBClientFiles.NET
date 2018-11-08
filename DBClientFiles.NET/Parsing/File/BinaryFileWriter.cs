using System.IO;
using System.Text;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.File
{
    /// <summary>
    /// An abstract specialization of <see cref="BinaryFileReader{T}"/> for record types that have a key.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="T"></typeparam>
    internal abstract class BinaryFileWriter<TKey, T> : BinaryFileWriter<T>
    {
        public BinaryFileWriter(StorageOptions options, Stream input, bool leaveOpen) : base(options, input, leaveOpen)
        {
            RegisterBlockHandler<CopyTableHandler<TKey>>();
        }

        public abstract ISerializer<TKey, T> KeySerializer { get; }
        public override ISerializer<T> Serializer => KeySerializer;
    }

    /// <summary>
    /// A basic implementation of the <see cref="IWriter{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    internal abstract class BinaryFileWriter<T> : BinaryReader, IWriter<T>
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
        public BinaryFileWriter(StorageOptions options, Stream input, bool leaveOpen) : base(input, Encoding.UTF8, leaveOpen)
        {
            Type = new TypeInfo(typeof(T));
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

        protected virtual IRecordReader GetRecordWriter()
        {
            return null;
            // return new BaseRecordReader(this, Header.RecordSize, BaseStream);
        }

        public abstract ISerializer<T> Serializer { get; }
    }
}
