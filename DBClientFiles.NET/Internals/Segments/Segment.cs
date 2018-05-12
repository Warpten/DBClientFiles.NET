using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Versions;
using System;

namespace DBClientFiles.NET.Internals.Segments
{
    internal class Segment<TValue, TReader> : Segment<TValue>
        where TValue : class, new()
        where TReader : ISegmentReader<TValue>, class, new()
    {
        public TReader Reader { get; }

        internal Segment(BaseReader<TValue> storage) : base(storage)
        {
            Reader = new TReader();
        }
    }

    internal class Segment<TValue> : IDisposable, IEquatable<Segment<TValue>>
        where TValue : class, new()
    {
        public long StartOffset { get; set; }
        public long Length { get; set; }

        public long EndOffset => StartOffset + Length;

        private bool _existsOverride;

        public BaseReader<TValue> Storage { get; private set; }
        private StorageOptions Options { get; set; }

        public bool Exists
        {
            set => _existsOverride = value;
            get
            {
                return _existsOverride && Length != 0;
            }
        }

        internal Segment(BaseReader<TValue> storage)
        {
            Options = storage.Options;
            Storage = storage;

            StartOffset = 0;
            Length = 0;

            _existsOverride = false;
        }

        public void Dispose()
        {
            Options = null;
            Storage = null;
        }

        public bool Equals(Segment<TValue> other)
        {
            return StartOffset == other.StartOffset && Length == other.Length;
        }
    }
}
