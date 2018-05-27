using System;
using System.IO;
using DBClientFiles.NET.Internals.Versions;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader that treats the entirety of its content as byte data that is to be deserialized as need may be.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    internal sealed class BinarySegmentReader<TValue> : ISegmentReader<TValue>
        where TValue : class, new()
    {
        public FileReader Storage => Segment.Storage;

        public Segment<TValue> Segment { get; set; }

        private byte[] _data;

        public void Dispose()
        {
            _data = null;
        }

        public void Read()
        {
            if (Segment.Length == 0)
                return;

            Storage.BaseStream.Position = Segment.StartOffset;
            _data = Storage.ReadBytes((int)Segment.Length);
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
