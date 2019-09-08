using System.Collections.Generic;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using System.Diagnostics;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    internal class IndexTableEnumerator<TValue> : DecoratingEnumerator<TValue>
    {
        private readonly IndexTableHandler _blockHandler;
        private int _cursor;

        public IndexTableEnumerator(Enumerator<TValue> impl) : base(impl)
        {
            _blockHandler = Parser.FindSegmentHandler<IndexTableHandler>(SegmentIdentifier.IndexTable);
            Debug.Assert(_blockHandler != null, "Block handler missing for index table");
            _cursor = 0;
        }

        internal override TValue ObtainCurrent()
        {
            var instance = base.ObtainCurrent();
            if (EqualityComparer<TValue>.Default.Equals(instance, default))
                return default;

            Parser.SetRecordKey(out instance, _blockHandler[_cursor]);
            ++_cursor;
            return instance;
        }

        public override void Reset()
        {
            base.Reset();

            _cursor = 0;
        }

        public override Enumerator<TValue> WithIndexTable()
        {
            return this;
        }
    }
}
