using System;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Versions;
using System.Diagnostics;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    internal class RecordsEnumerator<TParser, TValue> : Enumerator<TParser, TValue>
        where TParser : BinaryStorageFile<TValue>
    {
        private Segment _segment;

        public RecordsEnumerator(TParser impl) : base(impl)
        {
            _segment = Parser.FindSegment(SegmentIdentifier.Records);
            Debug.Assert(_segment != null, "Records block missing in enumerator");

            Parser.DataStream.Position = _segment.StartOffset;
        }

        internal override TValue ObtainCurrent()
        {
            if (Parser.DataStream.Position >= _segment.EndOffset)
                return default;

            return Parser.ObtainRecord(Parser.DataStream.Position, Parser.Header.RecordSize);
        }

        public override void Reset()
        {
            base.Reset();

            Parser.DataStream.Position = _segment.StartOffset;
        }

        public override void Skip(int skipCount)
        {
            Parser.DataStream.Position += Parser.Header.RecordSize * skipCount;
        }

        public override TValue ElementAt(int index)
        {
            var offset = Parser.Header.RecordSize * index;
            if (offset >= _segment.EndOffset)
                throw new ArgumentOutOfRangeException(nameof(index));

            return Parser.ObtainRecord(offset, Parser.Header.RecordSize);
        }

        public override TValue ElementAtOrDefault(int index)
        {
            var offset = Parser.Header.RecordSize * index;
            if (offset >= _segment.EndOffset)
                return default;

            return Parser.ObtainRecord(offset, Parser.Header.RecordSize);
        }
    }
}
