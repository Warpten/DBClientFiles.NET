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
        private Memory<byte> _memorySpan;

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

            _memorySpan = new Memory<byte>(_data, 0, _data.Length);
        }

        public unsafe T[] ReadArray<T>(long offset, int arraySize) where T : struct
        {
            var buffer = new T[arraySize];
            fixed (byte* b = _data)
                FastStructure.ReadArray(buffer, new IntPtr(b + offset), 0, arraySize);
            return buffer;
        }

        public T Read<T>(int offset) where T : struct
        {
            var span = MemoryMarshal.Cast<byte, T>(_memorySpan.Slice(offset, SizeCache<T>.Size).Span);
            return span[0];

            //fixed (byte* ptr = _data)
            //    return FastStructure.PtrToStructure<T>(new IntPtr(ptr + offset));
        }
    }
}
