using System;

namespace DBClientFiles.NET.Internals.Segments
{
    internal struct Segment<TValue> : IEquatable<Segment<TValue>>
        where TValue : class, new()
    {
        public long StartOffset { get; set; }
        public long Length { get; set; }

        public long EndOffset => StartOffset + Length;
        public int ItemLength { get; set; }

        public bool Exists {
            set {
                if (!value)
                    Length = 0;
            }
            get => Length != 0;
        }

        public bool Equals(Segment<TValue> other)
        {
            return StartOffset == other.StartOffset && Length == other.Length;
        }
    }
}
