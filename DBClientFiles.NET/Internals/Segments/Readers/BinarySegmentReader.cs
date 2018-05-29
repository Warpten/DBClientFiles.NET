using System;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader that treats the entirety of its content as byte data that is to be deserialized as need may be.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    internal sealed class BinarySegmentReader<TValue> : SegmentReader<TValue>
        where TValue : class, new()
    {
        private byte[] _data;

        public BinarySegmentReader(FileReader reader) : base(reader) { }

        protected override void Release()
        {
            _data = null;
        }

        public override void Read()
        {
            if (Segment.Length == 0)
                return;

            FileReader.BaseStream.Position = Segment.StartOffset;
            _data = FileReader.ReadBytes((int)Segment.Length);
        }

        public unsafe T[] ReadArray<T>(long offset, int arraySize) where T : struct
        {
            var buffer = new T[arraySize];
            fixed (byte* b = _data)
                FastStructure.ReadArray(buffer, new IntPtr(b + offset), 0, arraySize);
            return buffer;
        }

        public unsafe T Read<T>(long offset) where T : struct
        {
            fixed (byte* ptr = _data)
                return FastStructure.PtrToStructure<T>(new IntPtr(ptr + offset));
        }
    }
}
