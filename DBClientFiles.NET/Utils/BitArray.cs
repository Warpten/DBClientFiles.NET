using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Utils
{
    internal struct BitArray
    {
        public static implicit operator BitArray(Memory<byte> memory)
        {
            return new BitArray(memory);
        }

        private Memory<byte> _storage;

        public static BitArray Create(Stream stream, int size)
        {
            var buffer = new byte[size + 8];
            var actualSize = stream.Read(buffer, 0, size);
            
            return new Memory<byte>(buffer, 0, actualSize + 8);
        }

        public static BitArray Create(Memory<byte> memory) => memory;

        private BitArray(Memory<byte> buffer)
        {
            _storage = buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T ReadAt<T>(int bitOffset) where T : unmanaged
        {
            var ptr = (ulong*) Unsafe.AsPointer(ref _storage.Span[bitOffset / 8]);
            var value = (*ptr << (64 - sizeof(T) - (bitOffset & 7))) >> (64 - sizeof(T));
            return *(T*)&value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T ReadAt<T>(int bitOffset, int bitCount) where T : unmanaged
        {
            var value = ReadAt<T>(bitOffset);
            var valuePtr = (ulong*) Unsafe.AsPointer(ref value);
            var shiftedValue = *valuePtr >> (sizeof(T) - bitCount);
            return *(T*)&shiftedValue;
        }
    }
}
