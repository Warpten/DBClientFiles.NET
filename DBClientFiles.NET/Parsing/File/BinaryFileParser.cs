using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.File
{
    /// <summary>
    /// A basic implementation of the <see cref="IParser"/> interface.
    /// </summary>
    /// <typeparam name="TValue">The record type.</typeparam>
    /// <typeparam name="TSerializer"></typeparam>
    internal abstract class BinaryFileParser<TValue, TSerializer> : IParser<TValue>
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

        public IHeaderHandler Header => (IHeaderHandler) Head.Handler;

        /// <summary>
        /// Options used for parsing the file.
        /// </summary>
        private StorageOptions _options;

        public Stream BaseStream { get; }

        /// <summary>
        /// Create an instance of <see cref="BinaryFileParser{TValue, TSerializer}"/>.
        /// </summary>
        /// <param name="options">The options to use for parsing.</param>
        /// <param name="input">The input stream.</param>
        /// <param name="leaveOpen">If <c>true</c>, the stream is left open once this object is disposed.</param>
        public BinaryFileParser(in StorageOptions options, Stream input)
        {
            BaseStream = input;

            if (!input.CanSeek)
                throw new ArgumentException("The stream provided to DBClientFiles.NET's collections has to be seekable!");

            Type = new TypeToken(typeof(TValue));
            _options = options;

            Serializer = new TSerializer();
        }

        public virtual void Dispose()
        {
            Head = default;
        }

        public Block FindBlock(BlockIdentifier identifier)
        {
            var blck = Head;
            while (blck != null && blck.Identifier != identifier)
                blck = blck.Next;

            return blck;
        }

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

        //! TODO: Abstract
        public virtual IEnumerator<TValue> GetEnumerator()
        {
            var copyTableHandler = FindBlock(BlockIdentifier.CopyTable)?.Handler as CopyTableHandler;
            if (copyTableHandler != null)
                return new CopyTableEnumerator<TValue, TSerializer>(this, copyTableHandler);

            return new Enumerator<TValue, TSerializer>(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
