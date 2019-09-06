using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.Versions
{
    /// <summary>
    /// A basic implementation of the <see cref="IBinaryStorageFile{T}"/> interface.
    /// </summary>
    /// <typeparam name="TValue">The record type.</typeparam>
    /// <typeparam name="TSerializer"></typeparam>
    internal abstract class BinaryStorageFile<TValue> : IBinaryStorageFile<TValue>
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
        /// This is the total number of elements in the file.
        /// </summary>
        public abstract int RecordCount { get; }

        /// <summary>
        /// The first block of the file (typically its header).
        /// </summary>
        public Segment Head { get; protected set; }

        public IHeaderHandler Header => (IHeaderHandler) Head.Handler;

        /// <summary>
        /// Options used for parsing the file.
        /// </summary>
        private readonly StorageOptions _options;

        public Stream DataStream { get; }

        /// <summary>
        /// Create an instance of <see cref="BinaryStorageFile{TValue}"/>.
        /// </summary>
        /// <param name="options">The options to use for parsing.</param>
        /// <param name="input">The input stream.</param>
        protected BinaryStorageFile(in StorageOptions options, Stream input)
        {
            DataStream = input;

            if (!input.CanSeek)
                throw new ArgumentException("The stream provided to DBClientFiles.NET's collections has to be seekable!");

            Type = new TypeToken(typeof(TValue));
            _options = options;
        }

        public virtual void Dispose()
        {
            Head = default;
        }

        public Segment FindSegment(SegmentIdentifier identifier)
        {
            var blck = Head;
            while (blck != null && blck.Identifier != identifier)
                blck = blck.Next;

            return blck;
        }

        public T FindSegmentHandler<T>(SegmentIdentifier identifier) where T : ISegmentHandler
        {
            var block = FindSegment(identifier);
            if (block == default(Segment))
                return default;

            Debug.Assert(block.Handler is T, "Wrong type for block handler lookup");
            return (T) block.Handler;
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

        public IEnumerator<TValue> GetEnumerator()
        {
            DataStream.Position = 0;
            Before(ParsingStep.Segments);

            var head = Head;
            while (head != null)
            {
                if (!head.ReadSegment(this))
                    DataStream.Seek(head.Length, SeekOrigin.Current);

                head = head.Next;
            }

            After(ParsingStep.Segments);

            return CreateEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected abstract IEnumerator<TValue> CreateEnumerator();

        /// <summary>
        /// Tries to read an instance of <see cref="TValue"/> at the provided offset in the stream, and limiting reads to the length specified.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public abstract TValue ObtainRecord(long offset, long length);

        internal abstract int GetRecordKey(in TValue value);
        internal abstract void SetRecordKey(out TValue value, int recordKey);

        internal abstract void Clone(in TValue source, out TValue clonedInstance);
    }
}
