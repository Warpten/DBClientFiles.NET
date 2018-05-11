using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Versions;
using System;

namespace DBClientFiles.NET.Internals.Segments
{
    internal class Segment<TValue> : IDisposable, IEquatable<Segment<TValue>> where TValue : class, new()
    {
        public long StartOffset;
        public long Length;

        public long EndOffset => StartOffset + Length;

        private bool _existsOverride;

        public BaseReader<TValue> Reader { get; private set; }
        private StorageOptions Options { get; set; }

        public bool Exists
        {
            set => _existsOverride = value;
            get {
                return _existsOverride && Length != 0;
            }
        }

        public Segment(BaseReader<TValue> storage)
        {
            Reader = storage;
            Options = storage.Options;

            StartOffset = 0;
            Length = 0;

            _existsOverride = false;
        }

        public void Dispose()
        {
            Reader = null;
            Options = null;
        }

        public bool Equals(Segment<TValue> other)
        {
            return StartOffset == other.StartOffset && Length == other.Length;
        }

        public static implicit operator bool(Segment<TValue> segment)
        {
            return segment.Exists;
        }
    }
}
