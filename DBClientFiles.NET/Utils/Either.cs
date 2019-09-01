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
        private readonly L _left;

        [FieldOffset(0)]
        private readonly R _right;

        public L Left => _left;
        public R Right => _right;


        public static Either<L, R> FromRight(R right) => new Either<L, R>(right);
        public static Either<L, R> FromLeft(L left) => new Either<L, R>(left);

        private Either(L left)
        {
            Debug.Assert(Unsafe.SizeOf<L>() == Unsafe.SizeOf<R>(), "L and R must have the same size");

            _right = default;
            _left = left;
        }

        private Either(R right)
        {
            Debug.Assert(Unsafe.SizeOf<L>() == Unsafe.SizeOf<R>(), "L and R must have the same size");

            _left = default;
            _right = right;
        }
    }
}
