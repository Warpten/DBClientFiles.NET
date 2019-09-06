using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Utils
{
    /// <summary>
    /// A poor man's union.
    /// </summary>
    /// <typeparam name="L"></typeparam>
    /// <typeparam name="R"></typeparam>
    internal readonly ref struct Union<L, R>
        where L : struct
        where R : struct
    {
        private readonly Span<byte> _data;

        public L Left => MemoryMarshal.Read<L>(_data);
        public R Right => MemoryMarshal.Read<R>(_data);

        public static Union<L, R> FromRight(R right) => new Union<L, R>(right);
        public static Union<L, R> FromLeft(L left) => new Union<L, R>(left);

        public static implicit operator Union<L, R>(L value) => FromLeft(value);
        public static implicit operator Union<L, R>(R value) => FromRight(value);

        private Union(L left)
        {
            Debug.Assert(Unsafe.SizeOf<L>() == Unsafe.SizeOf<R>(), "L and R must have the same size");

            _data = new byte[Unsafe.SizeOf<L>()];
            MemoryMarshal.Write(_data, ref left);
        }

        private Union(R right)
        {
            Debug.Assert(Unsafe.SizeOf<L>() == Unsafe.SizeOf<R>(), "L and R must have the same size");

            _data = new byte[Unsafe.SizeOf<L>()];
            MemoryMarshal.Write(_data, ref right);
        }
    }
}
