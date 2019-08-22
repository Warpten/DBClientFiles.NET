using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DBClientFiles.NET.Parsing.File.Enumerators
{
    internal class OffsetMapEnumerator<TValue, TSerializer> : Enumerator<TValue, TSerializer>
        where TSerializer : ISerializer<TValue>, new()
    {
        private OffsetMapHandler _blockHandler;
        private int _cursor;

        public OffsetMapEnumerator(BinaryFileParser<TValue, TSerializer> impl) : base(impl)
        {
            _blockHandler = FileParser.FindBlockHandler<OffsetMapHandler>(BlockIdentifier.OffsetMap);
            _cursor = 0;
            Debug.Assert(_blockHandler != null, "Block handler missing for offset map");

        }

        internal override TValue ObtainCurrent()
        {
            if (_cursor == _blockHandler.Count)
                return default;

            var (offset, length) = _blockHandler[_cursor++];
            FileParser.BaseStream.Position = offset;

            var recordReader = FileParser.GetRecordReader(length);
            return Serializer.Deserialize(recordReader, FileParser);
        }

        internal override void ResetIterator()
        {
            _cursor = 0;
        }
    }
}
