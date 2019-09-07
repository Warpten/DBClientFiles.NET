using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Utils
{
    /// <summary>
    /// A variant is a structure encapsulating an amount of bytes and allowing to reinterpret those bytes as a given type.
    /// </summary>
    internal readonly ref struct Variant
    {
        private readonly ReadOnlySpan<byte> _data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Cast<T>() where T : struct => MemoryMarshal.Read<T>(_data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Variant(ReadOnlySpan<byte> data) => _data = data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Variant From<U>(U value) where U : struct => new Variant(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref value, 1)));
    }

    internal struct Variant<T> where T : struct
    {
        private T _data;

        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public U Cast<U>() where U : struct => MemoryMarshal.Read<U>(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref _data, 1)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Variant(Variant<T> variant) => Variant.From(variant._data);

        public static implicit operator T(Variant<T> variant) => variant._data;
    }
}
