using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader that produces an enumeration of keys for the given segment of DB2 files.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    internal sealed class IndexTableReader<TKey> : SegmentReader
        where TKey : struct
    {
        private TKey[] _keys;

        public IndexTableReader(FileReader reader) : base(reader) { }

        public override void Read()
        {
            if (Segment.Length == 0)
                return;

            FileReader.BaseStream.Position = Segment.StartOffset;
            _keys = FileReader.ReadStructs<TKey>(Segment.Length / SizeCache<TKey>.Size);
        }

        public TKey this[int index] => _keys[index];

        protected override void Release()
        {
            _keys = null;
        }
    }
}
