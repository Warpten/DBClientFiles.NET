using System;
using System.Runtime.InteropServices;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader that treats the entirety of its content as byte data that is to be deserialized as need may be.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    internal sealed class BinarySegmentReader : SegmentReader
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
            _data = FileReader.ReadBytes(Segment.Length);
        }

        public T[] ReadArray<T>(int offset, int arraySize) where T : struct
        {
            var buffer = new T[arraySize];
            for (var i = 0; i < arraySize; ++i)
                buffer[i] = Read<T>(offset + SizeCache<T>.Size * i);
            return buffer;
        }

        public unsafe T Read<T>(int offset) where T : struct
        {
            fixed (byte* ptr = _data)
               return FastStructure.PtrToStructure<T>(new IntPtr(ptr + offset));
        }
    }
}
