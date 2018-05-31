using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader that produces an enumeration of keys for the given segment of DB2 files.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal sealed class IndexTableReader<TKey, TValue> : SegmentReader<TValue>
        where TValue : class, new()
        where TKey : struct
    {
        private TKey[] _keys;

        public IndexTableReader(FileReader reader) : base(reader) { }

        public override void Read()
        {
            if (Segment.Length == 0)
                return;

            _keys = new TKey[Segment.Length / typeof(TKey).GetBinarySize()];

            FileReader.BaseStream.Position = Segment.StartOffset;
            for (var i = 0; i < _keys.Length; ++i)
                _keys[i] = FileReader.ReadStruct<TKey>();
        }

        public TKey this[int index] => _keys[index];

        protected override void Release()
        {
            _keys = null;
        }
    }
}
