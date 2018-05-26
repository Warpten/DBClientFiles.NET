using DBClientFiles.NET.Utils;
using System.Collections.Generic;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class IndexTableReader<TKey, TValue> : SegmentReader<TKey, TValue>
        where TValue : class, new()
    {
        public IndexTableReader() { }

        private TKey[] _keys;

        public override void Read()
        {
            if (Segment.Length == 0)
                return;

            _keys = new TKey[Segment.Length / typeof(TKey).GetBinarySize()];

            Storage.BaseStream.Position = Segment.StartOffset;
            for (var i = 0; i < _keys.Length; ++i)
                _keys[i] = Storage.ReadStruct<TKey>();
        }

        public TKey this[int index] => _keys[index];

        protected override void Release()
        {
            _keys = null;
        }
    }
}
