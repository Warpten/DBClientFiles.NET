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
        public T Read<T>(int bitCount) where T : unmanaged
        {
#if DEBUG
            if ((_bitCursor & 7) == 0)
            {
                if (bitCount != UnsafeCache<T>.Size * 8)
                    Console.WriteLine($"Reading {typeof(T).Name} (size {UnsafeCache<T>.Size * 8} bits, but packed to {bitCount} bits) at offset {_bitCursor / 8} / {_recordSize}");
                else
                    Console.WriteLine($"Reading {typeof(T).Name} at offset {_bitCursor / 8} / {_recordSize}");
            }
            else
            {

                if (bitCount != UnsafeCache<T>.Size * 8)
                    Console.WriteLine($"Reading {typeof(T).Name} (size {UnsafeCache<T>.Size * 8} bits, but packed to {bitCount} bits) at offset {_bitCursor / 8} / {_recordSize} (misaligned by { _bitCursor & 7} bits)");
                else
                    Console.WriteLine($"Reading {typeof(T).Name} at offset {_bitCursor / 8} / {_recordSize} (misaligned by { _bitCursor & 7} bits)");
            }
#endif

            var byteOffset = _bitCursor / 8;
            var byteCount = ((bitCount + (_bitCursor & 7) + 7)) / 8;

            Debug.Assert(byteOffset + byteCount <= _recordSize);

            unsafe
            {
                var longValue = ((*(ulong*)((byte*)_recordData + byteOffset)) << (64 - bitCount - (_bitCursor & 7))) >> (64 - bitCount);
                _bitCursor += bitCount;
                return *(T*)&longValue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged => Read<T>(Unsafe.SizeOf<T>() * 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ReadArray<T>(int count) where T : unmanaged
            => ReadArray<T>(count, Unsafe.SizeOf<T>() * 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ReadArray<T>(int count, int elementBitCount) where T : unmanaged
        {
            var value = new T[count];
            for (var i = 0; i < count; ++i)
                value[i] = Read<T>(elementBitCount);

            return value;
        }

        public virtual string ReadString(int bitCount)
        {
            var handler = _fileReader.FindBlock(BlockIdentifier.StringBlock)?.Handler as StringBlockHandler;
            var stringIdentifier = Read<uint>(bitCount);
            return handler[stringIdentifier];
        }

        public string ReadString()
            => ReadString(32);

        public string[] ReadStringArray(int count, int elementBitCount)
        {
            var value = new string[count];
            for (var i = 0; i < count; ++i)
                value[i] = ReadString(elementBitCount);
            return value;
        }

        public string[] ReadStringArray(int count)
        {
            var value = new string[count];
            for (var i = 0; i < count; ++i)
                value[i] = ReadString();
            return value;
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
