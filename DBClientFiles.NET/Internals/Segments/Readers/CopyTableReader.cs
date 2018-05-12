using System.Collections.Generic;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class CopyTableReader<TValue> : SegmentReader<(int, int), TValue>
        where TValue : class, new()
    {
        public CopyTableReader() { }

        public override IEnumerable<(int, int)> Enumerate()
        {
            throw new System.NotImplementedException();
        }

        public override void Read()
        {
            throw new System.NotImplementedException();
        }
    }
}
