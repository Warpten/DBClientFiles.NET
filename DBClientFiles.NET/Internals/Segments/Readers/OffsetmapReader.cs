using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class OffsetMapReader<TValue> : SegmentReader<TValue>
        where TValue : class, new()
    {
        public OffsetMapReader() { }

        private Dictionary<int, (long, int)> _parsedContent = new Dictionary<int, (long, int)>();

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
                var size = Storage.ReadInt16();

                ++i;

                if (offset == 0)
                    continue;

                _parsedContent.Add(MinIndex + i - 1, (offset, size));
            }
        }

        public long GetRecordOffset(int index)
        {
            return _parsedContent[index].Item1;
        }

        public int GetRecordSize(int index)
        {
            return _parsedContent[index].Item2;
        }

        protected override void Release()
        {
            _parsedContent.Clear();
        }

        public int Count => _parsedContent.Count;
    }
}
