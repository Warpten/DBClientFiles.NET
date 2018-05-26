using System.IO;
using DBClientFiles.NET.Internals.Versions;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class RawDataSegmentReader<TValue> : ISegmentReader<TValue>
        where TValue : class, new()
    {
        public BaseFileReader<TValue> Storage => Segment.Storage;

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

        public unsafe T[] ReadArray<T>(long offset, int arraySize)
        {
            fixed (byte* b = _data)
            {
                byte* c = b + offset;
                using (var ms = new UnmanagedMemoryStream(c, arraySize * typeof(T).GetBinarySize()))
                using (var reader = new BinaryReader(ms))
                    return reader.ReadStructs<T>(arraySize);
            }
        }

        public unsafe T Read<T>(long offset)
        {
            fixed (byte* b = _data)
            {
                byte* c = b + offset;
                using (var ms = new UnmanagedMemoryStream(c, typeof(T).GetBinarySize()))
                using (var reader = new BinaryReader(ms))
                    return reader.ReadStruct<T>();
            }
        }
    }
}
