using DBClientFiles.NET.IO;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DBClientFiles.NET.Parsing.File.Records
{
    internal unsafe class UnalignedRecordReader : IRecordReader
    {
        private readonly IntPtr _recordData;
        private int _bitCursor;
        private IBinaryStorageFile _fileReader;
        private int _recordSize;

        private bool _managePointer;

        public UnalignedRecordReader(IBinaryStorageFile fileReader, int recordSize)
        {
            _fileReader = fileReader;
            _recordSize = recordSize;

            // Allocating 7 extra bytes to guarantee we don't ever read out of our memory space
            // _recordData = Marshal.AllocHGlobal(recordData.Length + 7);
            _recordData = Marshal.AllocHGlobal(recordSize);
            _managePointer = true;
        }

        public void LoadStream(Stream dataStream, int recordSize)
        {
            _bitCursor = 0;
            using (var windowedStream = new WindowedStream(dataStream, recordSize))
            using (var outputStream = new IO.UnmanagedMemoryStream(_recordData, recordSize))
                windowedStream.CopyTo(outputStream, recordSize);
        }

        public void Dispose()
        {
            if (_managePointer)
                Marshal.FreeHGlobal(_recordData);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadImmediate<T>(int bitOffset, int bitCount) where T : unmanaged
        {
            var byteOffset = bitOffset / 8;
            var byteCount = ((bitCount + (bitOffset & 7) + 7)) / 8;

            Debug.Assert(byteOffset + byteCount <= _recordSize);

            unsafe
            {
                var longValue = ((*(ulong*)((byte*)_recordData + byteOffset)) << (64 - bitCount - (_bitCursor & 7))) >> (64 - bitCount);
                _bitCursor += bitCount;
                return *(T*)&longValue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            var value = ReadImmediate<T>(_bitCursor, Unsafe.SizeOf<T>() * 8);
            _bitCursor += Unsafe.SizeOf<T>() * 8;
            return value;
        }

        public virtual string ReadString(int bitCursor, int bitCount)
        {
            var handler = _fileReader.FindBlock(BlockIdentifier.StringBlock)?.Handler as StringBlockHandler;
            var stringIdentifier = ReadImmediate<uint>(bitCursor, bitCount);
            return handler[stringIdentifier];
        }

        public string ReadString()
        {
            var handler = _fileReader.FindBlock(BlockIdentifier.StringBlock)?.Handler as StringBlockHandler;
            var stringIdentifier = Read<uint>();
            return handler[stringIdentifier];
        }

        public string ReadInlineString()
        {
            var stringLength = 0;
            var startOffset = _bitCursor >> 3;

            sbyte* recordData = (sbyte*)_recordData;
            while (recordData[_bitCursor / 8] != 0)
            {
                _bitCursor += 8;
                ++stringLength;
            }

            var result = new string(recordData, 0, stringLength, Encoding.UTF8);
            _bitCursor += 8;
            return result;
        }
    }
}
