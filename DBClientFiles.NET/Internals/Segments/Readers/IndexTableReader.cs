using System;
using System.Runtime.InteropServices;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader that produces an enumeration of keys for the given segment of DB2 files.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    internal sealed class IndexTableReader : SegmentReader
    {
        private byte[] _segmentData;

        public IndexTableReader(FileReader reader) : base(reader) { }

        public override void Read()
        {
            if (Segment.Length == 0)
                return;

            FileReader.BaseStream.Position = Segment.StartOffset;
            _segmentData = FileReader.ReadBytes(Segment.Length);
        }

        public T GetValue<T>(int index)
            where T : struct
        {
            ReadOnlySpan<byte> block = _segmentData;
            var tBlock = MemoryMarshal.Cast<byte, T>(block);
            return tBlock[index];
        }

        protected override void Release()
        {
            _segmentData = null;
        }
    }
}
