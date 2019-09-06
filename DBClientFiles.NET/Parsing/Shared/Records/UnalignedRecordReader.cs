using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace DBClientFiles.NET.Parsing.Shared.Records
{
    internal unsafe class UnalignedRecordReader : IRecordReader
    {
        private readonly IMemoryOwner<byte> _recordData;
        private readonly IBinaryStorageFile _fileReader;
        private readonly StringBlockHandler _stringBlock;
        private readonly long _recordSize;

        private Span<byte> Span => _recordData.Memory.Span;

        public UnalignedRecordReader(IBinaryStorageFile fileReader, long recordSize)
        {
            _fileReader = fileReader;
            _recordSize = recordSize;

            // Allocating 7 extra bytes to guarantee we don't ever read out of our memory space
            _recordData = MemoryPool<byte>.Shared.Rent((int)(recordSize + 7));

            _stringBlock = _fileReader.FindSegment(SegmentIdentifier.StringBlock)?.Handler as StringBlockHandler;

            // Read exactly what we need
            fileReader.DataStream.Read(_recordData.Memory.Span.Slice(0, (int) recordSize));
        }

        public void Dispose()
        {
            _recordData.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadImmediate<T>(int bitOffset, int bitCount) where T : unmanaged
        {
            var byteOffset = bitOffset / 8;
            var byteCount = ((bitCount + (bitOffset & 7) + 7)) / 8;

            Debug.Assert(byteOffset + byteCount <= _recordSize);

            unsafe
            {
                var dataPtr = (ulong*) Unsafe.AsPointer(ref Span[byteOffset]);

                var longValue = (*dataPtr << (64 - bitCount - (bitOffset & 7))) >> (64 - bitCount);
                return *(T*)&longValue;
            }
        }

        public string ReadString(int bitCursor, int bitCount)
        {
            if (_stringBlock != null)
                return _stringBlock[ReadImmediate<uint>(bitCursor, bitCount)];

            Debug.Assert((bitCursor & 7) == 0);

            var stringLength = 0;
            var startOffset = bitCursor >> 3;

            sbyte* recordData = (sbyte*) Unsafe.AsPointer(ref Span[0]);
            while (recordData[bitCursor / 8] != 0)
            {
                bitCursor += 8;
                ++stringLength;
            }

            var result = new string(recordData, 0, stringLength, _fileReader.Options.Encoding ?? Encoding.UTF8);
            return result;
        }
    }
}
