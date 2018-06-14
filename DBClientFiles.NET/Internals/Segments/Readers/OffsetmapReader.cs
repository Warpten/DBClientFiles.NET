using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class OffsetMapReader : SegmentReader
    {
        private readonly Dictionary<int, (long, int)> _parsedContent = new Dictionary<int, (long, int)>();

        public OffsetMapReader(FileReader reader) : base(reader) { }

        public override void Read()
        {
            if (!Segment.Exists)
                return;

            HashSet<long> _knownOffsets = new HashSet<long>();

            var i = 0;
            FileReader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            while (FileReader.BaseStream.Position < Segment.EndOffset)
            {
                long offset = FileReader.ReadInt32();
                var size = FileReader.ReadInt16();

                if (offset == 0 || size == 0)
                    continue;

                // offset map can contain duplicates which should be excluded
                if (!_knownOffsets.Contains(offset))
                {
                    _parsedContent.Add(i++, (offset, size));
                    _knownOffsets.Add(offset);
                }                    
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
