using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions;
using System.Diagnostics;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    internal class OffsetMapEnumerator<TValue, TSerializer> : Enumerator<TValue, TSerializer>
        where TSerializer : ISerializer<TValue>, new()
    {
        private OffsetMapHandler _blockHandler;
        private int _cursor;

        public OffsetMapEnumerator(BinaryFileParser<TValue, TSerializer> impl) : base(impl)
        {
            _blockHandler = Parser.FindSegmentHandler<OffsetMapHandler>(SegmentIdentifier.OffsetMap);
            _cursor = 0;
            Debug.Assert(_blockHandler != null, "Block handler missing for offset map");

        }

        internal override TValue ObtainCurrent()
        {
            if (_cursor == _blockHandler.Count)
                return default;

            var (offset, length) = _blockHandler[_cursor++];
            Parser.BaseStream.Position = offset;

            var recordReader = Parser.GetRecordReader(length);
            return Serializer.Deserialize(recordReader, Parser);
        }

        internal override void ResetIterator()
        {
            _cursor = 0;
        }
    }
}
