using System;
using System.Collections.Generic;
using System.Linq;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader that treats the entirety of its content as byte data that is to be deserialized as need may be.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    internal sealed class PalletSegmentReader : SegmentReader
    {
        private SegmentBlock[] _segments;

        private struct SegmentBlock
        {
            private byte[] _blockData;

            public SegmentBlock(FileReader reader, int segmentLength)
            {
                _blockData = reader.ReadBytes(segmentLength);
            }

            public unsafe T ExtractValue<T>(int offset) where T : struct
            {
                fixed (byte* buffer = _blockData)
                    return FastStructure.PtrToStructure<T>(new IntPtr(buffer + offset));
            }
        }

        public PalletSegmentReader(FileReader reader) : base(reader) { }

        protected override void Release()
        {
            _segments = null;
        }

        public void Initialize(IEnumerable<int> blockLengths)
        {
            if (Segment.Length == 0)
                return;

            FileReader.BaseStream.Position = Segment.StartOffset;
            var blockSizes = blockLengths.ToArray();
            _segments = new SegmentBlock[blockSizes.Length];
            for (var i = 0; i < _segments.Length; ++i)
                _segments[i] = new SegmentBlock(FileReader, blockSizes[i]);
        }

        public override void Read()
        {

        }

        public T[] ReadArray<T>(int blockIndex, int offset, int arraySize) where T : struct
        {
            var buffer = new T[arraySize];
            for (var i = 0; i < arraySize; ++i)
                buffer[i] = Read<T>(blockIndex, offset + SizeCache<T>.Size * i);
            return buffer;
        }

        public unsafe T Read<T>(int blockIndex, int offset) where T : struct
        {
            return _segments[blockIndex].ExtractValue<T>(offset);
        }
    }
}
