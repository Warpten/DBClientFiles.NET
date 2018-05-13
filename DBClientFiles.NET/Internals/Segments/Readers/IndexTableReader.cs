using System.Collections.Generic;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class IndexTableReader<TValue> : SegmentReader<int, TValue>
        where TValue : class, new()
    {
        public IndexTableReader() { }

        public override IEnumerable<int> Enumerate()
        {
            throw new System.NotImplementedException();
        }

        public override void Read()
        {
            throw new System.NotImplementedException();
        }

        protected override void Release()
        {
            throw new System.NotImplementedException();
        }
    }
}
