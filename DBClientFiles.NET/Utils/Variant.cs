using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Utils
{
    internal struct Variant<T> where T : struct
    {
        public T Value;

        public Variant(T value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public U Cast<U>() where U : struct => Unsafe.As<T, U>(ref Value);

        public static explicit operator Variant<T>(T value) => new (value);
    }
}
