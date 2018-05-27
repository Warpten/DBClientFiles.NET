using System;
using System.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class RecordReader<TValue> : SegmentReader<TValue>
        where TValue : class, new()
    {
        private byte[] _data;

        public int RecordSize { get; set; }
        public int RecordCount => _data.Length / RecordSize;

        public override void Read()
        {
            if (!Segment.Exists)
                return;

            Storage.BaseStream.Position = Segment.StartOffset;
            _data = Storage.ReadBytes((int)Segment.Length);
        }

        public unsafe UnmanagedMemoryStream GetRecord(int recordIndex)
        {
            fixed (byte* b = _data)
            {
                byte* r = b + recordIndex * RecordSize;
                return new UnmanagedMemoryStream(r, RecordSize);
            }
        }

        protected override void Release()
        {
            throw new NotImplementedException();
        }
    }
}
