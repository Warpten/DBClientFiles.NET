using DBClientFiles.NET.Utils;
using System.Collections.Generic;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader that produces an enumeration of keys for the given segment of DB2 files.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal sealed class IndexTableReader<TKey, TValue> : SegmentReader<TValue>
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
