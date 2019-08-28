using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Versions;
using System.Diagnostics;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    class RecordsEnumerator<TValue, TSerializer> : Enumerator<TValue, TSerializer>
        where TSerializer : ISerializer<TValue>, new()
    {
        private Segment _segment;

        public RecordsEnumerator(BinaryFileParser<TValue, TSerializer> impl) : base(impl)
        {
            _segment = Parser.FindSegment(SegmentIdentifier.Records);
            Debug.Assert(_segment != null, "Records block missing in enumerator");

            Parser.BaseStream.Position = _segment.StartOffset;
        }

        internal override TValue ObtainCurrent()
        {
            if (Parser.BaseStream.Position >= _segment.EndOffset)
                return default;

            var recordReader = Parser.GetRecordReader(Parser.Header.RecordSize);
            return Serializer.Deserialize(recordReader, Parser);
        }

        internal override void ResetIterator()
        {
            Parser.BaseStream.Position = _segment.StartOffset;
        }
    }
}
