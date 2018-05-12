using DBClientFiles.NET.Internals.Versions;
using System.Collections.Generic;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal interface ISegmentReader<TValue> where TValue : class, new()
    {
        Segment<TValue> Segment { get; }
        void Read();
    }

    internal abstract class SegmentReader<T, TValue> : ISegmentReader<TValue> where TValue : class, new()
    {
        private Segment<TValue> _segment;
        public Segment<TValue> Segment => _segment;
        protected BaseReader<TValue> Storage => _segment.Storage;

        protected SegmentReader()
        {

        }

        protected SegmentReader(Segment<TValue> segment)
        {
            _segment = segment;
        }

        public abstract IEnumerable<T> Enumerate();
        public abstract void Read();
    }
}
