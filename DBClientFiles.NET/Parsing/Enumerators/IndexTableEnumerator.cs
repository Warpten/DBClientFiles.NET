using System.Collections.Generic;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using System.Diagnostics;
using DBClientFiles.NET.Parsing.Runtime.Serialization;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    internal class IndexTableEnumerator<TValue> : DecoratingEnumerator<TValue>
    {
        private readonly IndexTableHandler _blockHandler;
        private readonly RecordKeyAccessor<TValue> _keyAccessor;
        private int _cursor;

        public IndexTableEnumerator(Enumerator<TValue> impl, RecordKeyAccessor<TValue> keyAccessor) : base(impl)
        {
            _blockHandler = Parser.FindSegmentHandler<IndexTableHandler>(SegmentIdentifier.IndexTable);
            Debug.Assert(_blockHandler != null, "Block handler missing for index table");

            _keyAccessor = keyAccessor;

            _cursor = 0;
        }

        internal override TValue ObtainCurrent()
        {
            var instance = base.ObtainCurrent();
            if (EqualityComparer<TValue>.Default.Equals(instance, default))
                return default;

            _keyAccessor.SetRecordKey(out instance, _blockHandler[_cursor]);
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
