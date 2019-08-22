using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace DBClientFiles.NET.Utils
{
    internal struct BitArray
    {
        public static implicit operator BitArray(Memory<byte> memory)
        {
            return new BitArray(memory);
        }

        private Memory<byte> _storage;

        public int Offset { get; set; }

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

            Offset = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged => ReadAt<T>(Offset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(int bitCount) where T : unmanaged => ReadAt<T>(Offset, bitCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T ReadAt<T>(int bitOffset) where T : unmanaged => ReadAt<T>(bitOffset, Unsafe.SizeOf<T>() * 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T ReadAt<T>(int bitOffset, int bitCount) where T : unmanaged
        {
            var byteOffset = bitOffset / 8;

            var shiftBase = 64 - bitCount;
            
            // TODO: Ensure the bounds check is eliminated here
            var longValue = (*(ulong*)_storage.Span[byteOffset] << (shiftBase - (bitOffset & 7))) >> shiftBase;
            return *(T*) longValue;
        }
    }
}
