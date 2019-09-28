using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Utils
{
    internal struct Variant<T> where T : struct
    {
        public T Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public U Cast<U>() where U : struct => Unsafe.As<T, U>(ref Value);
    }
}
