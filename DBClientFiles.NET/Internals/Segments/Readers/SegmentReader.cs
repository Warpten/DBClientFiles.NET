using DBClientFiles.NET.Internals.Versions;
using System.Collections.Generic;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal interface ISegmentReader<TValue> where TValue : class, new()
    {
        Segment<TValue> Segment { get; set; }
        void Read();
    }

    internal abstract class SegmentReader<T, TValue> : ISegmentReader<TValue> where TValue : class, new()
    {
        private Segment<TValue> _segment;
        public Segment<TValue> Segment
        {
            get => _segment;
            set => _segment = value;
        }

        internal BaseReader<TValue> Storage => _segment.Storage;

        protected SegmentReader()
        {

        }

        public abstract IEnumerable<T> Enumerate();
        public abstract void Read();
    }
}
