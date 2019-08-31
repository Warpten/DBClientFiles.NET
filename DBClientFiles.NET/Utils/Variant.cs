using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DBClientFiles.NET.Utils
{
    /// <summary>
    /// A variant is a structure encapsulating an amount of bytes and allowing to reinterpret those bytes as a given type.
    /// </summary>
    internal readonly ref struct Variant
    {
        private readonly ReadOnlySpan<byte> _data;

        public long Int64
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.Read<long>(_data);
        }

        public ulong UInt64
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.Read<ulong>(_data);
        }

        public int Int32
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.Read<int>(_data);
        }

        public uint UInt32
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.Read<uint>(_data);
        }

        public short Int16
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.Read<short>(_data);
        }

        public ushort UInt16
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.Read<ushort>(_data);
        }

        public float Single
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.Read<float>(_data);
        }

        public double Double
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.Read<double>(_data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Cast<T>() where T : struct => MemoryMarshal.Read<T>(_data);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Variant(byte[] data) => _data = data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Variant(Span<byte> data) => _data = data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Variant(byte[] data) => new Variant(data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Variant(Span<byte> data) => new Variant(data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(Variant variant) => variant._data;
    }
}
