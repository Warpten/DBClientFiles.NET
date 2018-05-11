using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class OffsetmapReader<TValue> : SegmentReader<(int, long), TValue> where TValue : class, new()
    {
        public OffsetmapReader(Segment<TValue> segment) : base(segment)
        {
        }

        private Dictionary<int, long> _parsedContent = new Dictionary<int, long>();

        public int MinIndex { get; set; }
        public int MaxIndex { get; set; }

        public override IEnumerable<(int, long)> Enumerate()
        {
            if (!Segment.Exists)
                yield break;

            int i = 0;
            Reader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            while (Reader.BaseStream.Position < Segment.EndOffset)
            {
                long offset = Reader.ReadInt32();
                Reader.BaseStream.Seek(2, SeekOrigin.Current);

                ++i;

                if (offset == 0)
                    continue;

                _parsedContent.Add(MinIndex + i - 1, offset);
                yield return (MinIndex + i - 1, offset);
            }
        }

        public override void Read()
        {
            if (!Segment.Exists)
                return;

            int i = 0;
            Reader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            while (Reader.BaseStream.Position < Segment.EndOffset)
            {
                long offset = Reader.ReadInt32();
                Reader.BaseStream.Seek(2, SeekOrigin.Current);

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
    }
}
