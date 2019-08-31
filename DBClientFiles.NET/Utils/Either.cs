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
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct Either<L, R>
        where L : struct
        where R : struct
    {
        [FieldOffset(0)]
        public readonly L Left;
        [FieldOffset(0)]
        public readonly R Right;

        public Either(L left)
        {
            Debug.Assert(Unsafe.SizeOf<L>() == Unsafe.SizeOf<R>(), "L and R must have the same size");
            
            Right = default;
            Left = left;
        }

        public Either(R right)
        {
            Left = default;
            Right = right;
        }
    }
}
