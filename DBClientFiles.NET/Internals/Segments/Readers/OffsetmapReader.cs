using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class OffsetMapReader : SegmentReader
    {
        private readonly List<(long, int)> _parsedContent = new List<(long, int)>();

        public OffsetMapReader(FileReader reader) : base(reader) { }

        public override void Read()
        {
            if (!Segment.Exists)
                return;

            var _content = new HashSet<(long, int)>();

            FileReader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            while (FileReader.BaseStream.Position < Segment.EndOffset)
            {
                long offset = FileReader.ReadInt32();
                var size = FileReader.ReadInt16();

                if (offset == 0 || size == 0)
                    continue;

                _content.Add((offset, size));
            }

            _parsedContent.AddRange(_content);
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
