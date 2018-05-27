using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class OffsetMapReader<TValue> : SegmentReader<TValue>
        where TValue : class, new()
    {
        public OffsetMapReader() { }

        private Dictionary<int, long> _parsedContent = new Dictionary<int, long>();

        public int MinIndex { get; set; }
        public int MaxIndex { get; set; }

        public override void Read()
        {
            if (!Segment.Exists)
                return;

            int i = 0;
            Storage.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            while (Storage.BaseStream.Position < Segment.EndOffset)
            {
                long offset = Storage.ReadInt32();
                Storage.BaseStream.Seek(2, SeekOrigin.Current);

                ++i;

                if (offset == 0)
                    continue;

                _parsedContent.Add(MinIndex + i - 1, offset);
            }
        }

        public long this[int index]
        {
            get => _parsedContent[index];
        }

        protected override void Release()
        {
            _parsedContent.Clear();
        }
    }
}
