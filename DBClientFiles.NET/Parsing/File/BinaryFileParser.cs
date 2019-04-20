using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Utils;

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
        /// <summary>
        /// A representation of the type deserialized by this parser.
        /// </summary>
        public TypeToken Type { get; }

        /// <summary>
        /// The options to use for parsing.
        /// </summary>
        public ref readonly StorageOptions Options => ref _options;

        /// <summary>
        /// The header of the current file.
        /// </summary>
        public abstract ref readonly IFileHeader Header { get; }

        /// <summary>
        /// The actual deserializer.
        /// </summary>
        public TSerializer Serializer { get; }

        /// <summary>
        /// This is the total number of elements in the file.
        /// </summary>
        public abstract int RecordCount { get; }

        /// <summary>
        /// The first block of the file (typically its header).
        /// </summary>
        public Block Head { get; protected set; }

        /// <summary>
        /// Options used for parsing the file.
        /// </summary>
        private StorageOptions _options;

        /// <summary>
        /// Various handlers for each possible block encountered with the file.
        /// </summary>
        private BlockHandlers _handlers;

        /// <summary>
        /// Create an instance of <see cref="BinaryFileParser{TValue, TSerializer}"/>.
        /// </summary>
        /// <param name="options">The options to use for parsing.</param>
        /// <param name="input">The input stream.</param>
        /// <param name="leaveOpen">If <c>true</c>, the stream is left open once this object is disposed.</param>
        public BinaryFileParser(in StorageOptions options, Stream input) : base(input, Encoding.UTF8, true)
        {
            if (!input.CanSeek)
                throw new ArgumentException("The stream provided to DBClientFiles.NET's collections has to be seekable!");

            Type = new TypeToken(typeof(TValue));
            _options = options;

            Serializer = new TSerializer();
        }

        protected override void Dispose(bool disposing)
        {
            Head = default;
            _handlers.Clear();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Tries to find the first block matching the specified <see cref="BlockIdentifier"/>.
        /// 
        /// If none is found, returns null.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public Block FindBlock(BlockIdentifier identifier)
        {
            var itr = Head;
            while (itr != null && itr.Identifier != identifier)
                itr = itr.Next;
            return itr.Identifier == identifier ? itr : default;
        }

        /// <summary>
        /// Tries to find the block handler registered for the specified <see cref="BlockIdentifier"/>.
        /// 
        /// If none is found, returns null.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public U FindBlockHandler<U>(BlockIdentifier identifier) where U : IBlockHandler
            => _handlers.GetHandler<U>(identifier);

        public void RegisterBlockHandler<U>() where U : IBlockHandler, new()
            => RegisterBlockHandler(new U());
        public void RegisterBlockHandler(IBlockHandler handler)
            => _handlers.Register(handler);


        public T GetHandler<T>(BlockIdentifier identifier) where T : IBlockHandler
            => _handlers.GetHandler<T>(identifier);
    
        public bool ReadBlock<T>(T file, Block block) where T : BinaryReader, IParser
            => _handlers.ReadBlock(file, block);
    
        /// <summary>
        /// Called before a parsing step is executed.
        /// </summary>
        /// <param name="step"></param>
        public abstract void Before(ParsingStep step);

        /// <summary>
        /// Called after a parsing step is executed.
        /// </summary>
        /// <param name="step"></param>
        public abstract void After(ParsingStep step);

        /// <summary>
        /// Obtains an instance of <see cref="IRecordReader"/> used for record reading.
        /// </summary>
        /// <returns></returns>
        public abstract IRecordReader GetRecordReader(int recordSize);

        public IEnumerator<TValue> GetEnumerator()
        {
            var copyTableHandler = _handlers.GetHandler<CopyTableHandler>(BlockIdentifier.CopyTable);
            if (copyTableHandler != null)
                return new CopyTableEnumerator<TValue, TSerializer>(this, copyTableHandler);

            return new Enumerator<TValue, TSerializer>(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
