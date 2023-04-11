using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions;
using DBClientFiles.NET.Utils;
using System;
using System.Buffers;
using System.Collections.Immutable;
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
        private readonly PalletBlockHandler _palletBlock;
        private readonly long _recordSize;

        private Span<byte> Span => _recordData.Memory.Span;

        public UnalignedRecordReader(IBinaryStorageFile fileReader, long recordSize, StringBlockHandler stringBlock, PalletBlockHandler palletBlock)
        {
            _fileReader = fileReader;
            _recordSize = recordSize;

            // Allocating 7 extra bytes to guarantee we don't ever read out of our memory space
            _recordData = MemoryPool<byte>.Shared.Rent((int)(recordSize + 7));

            _stringBlock = stringBlock;
            _palletBlock = palletBlock;

            // Read exactly what we need
            fileReader.DataStream.Read(_recordData.Memory.Span.Slice(0, (int) recordSize));
        }

        public void Dispose()
        {
            _recordData.Dispose();
        }

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

        public T ReadPallet<T>(int bitOffset, int bitCount) where T : struct
        {
            var palletIndex = ReadImmediate<int>(bitOffset, bitCount) * sizeof(int);

            return _palletBlock.Read<T>(palletIndex);
        }

        public T ReadCommon<T>(int rawDefaultValue) where T : struct
        {
            var defaultValue = new Variant<int>(rawDefaultValue);
            return _commonBlock.Read<T>(defaultValue);
        }

        public T[] ReadPalletArray<T>(int bitOffset, int bitCount, int arraySize) where T : struct
        {
            var palletIndex = ReadImmediate<int>(bitOffset, bitCount) * sizeof(int);

            return _palletBlock.ReadArray<T>(palletIndex, arraySize);
        }

        public string ReadString(int bitCursor, int bitCount)
        {
            if (_stringBlock != null)
                return _stringBlock.ReadString(ReadImmediate<uint>(bitCursor, bitCount));

            Debug.Assert((bitCursor & 7) == 0);

            var stringLength = 0;
            var startOffset = bitCursor >> 3;

            sbyte* recordData = (sbyte*) Unsafe.AsPointer(ref Span[0]);
            while (recordData[bitCursor / 8] != 0)
            {
                bitCursor += 8;
                ++stringLength;
            }

            return new string(recordData, 0, stringLength, _fileReader.Options.Encoding ?? Encoding.UTF8);
        }

        public ReadOnlyMemory<byte> ReadUTF8(int bitCursor, int bitCount)
        {
            if (_stringBlock != null)
                return _stringBlock.ReadUTF8(ReadImmediate<int>(bitCursor, bitCount));

            Debug.Assert((bitCursor & 7) == 0);

            var stringLength = 0;
            var startOffset = bitCursor >> 3;

            sbyte* recordData = (sbyte*)Unsafe.AsPointer(ref Span[0]);
            while (recordData[bitCursor / 8] != 0)
            {
                bitCursor += 8;
                ++stringLength;
            }

            return Span[..stringLength].ToImmutableArray().AsMemory();
        }
    }
}
