using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions;
using System;
using System.Diagnostics;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    internal class OffsetMapEnumerator<TParser, TValue> : Enumerator<TParser, TValue>
        where TParser : BinaryStorageFile<TValue>
    {
        private readonly OffsetMapHandler _blockHandler;
        private int _cursor;

        public OffsetMapEnumerator(TParser impl) : base(impl)
        {
            _blockHandler = Parser.FindSegmentHandler<OffsetMapHandler>(SegmentIdentifier.OffsetMap);
            _cursor = 0;
            Debug.Assert(_blockHandler != null, "Block handler missing for offset map");

        }

        internal override TValue ObtainCurrent()
        {
            if (_cursor >= _blockHandler.Count)
                return default;

            var (offset, length) = _blockHandler[_cursor++];
            return Parser.ObtainRecord(offset, length);
        }

        public override void Reset()
        {
            base.Reset();

            _cursor = 0;
        }

        public override void Skip(int skipCount)
        {
            _cursor += skipCount;
        }

        public override TValue ElementAt(int index)
        {
            if (index >= _blockHandler.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var (offset, length) = _blockHandler[index];
            return Parser.ObtainRecord(offset, length);
        }

        public override TValue ElementAtOrDefault(int index)
        {
            if (index >= _blockHandler.Count)
                return default;

            var (offset, length) = _blockHandler[index];
            return Parser.ObtainRecord(offset, length);
        }

        public override TValue Last()
        {
            var (offset, length) = _blockHandler[_blockHandler.Count - 1];
            return Parser.ObtainRecord(offset, length);
        }
    }
}
