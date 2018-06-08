using System;
using System.Reflection.Emit;
// ReSharper disable StaticMemberInGenericType

namespace DBClientFiles.NET.Utils
{
    /// <summary>
    ///     A cheaty way to get very fast "Marshal.SizeOf" support without the overhead of the Marshaler each time.
    ///     Also provides a way to get the pointer of a generic type (useful for fast memcpy and other operations)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class SizeCache<T> where T : struct
    {
        /// <summary> The size of the Type </summary>
        public static readonly int Size;

        public static readonly int UnmanagedSize;

        /// <summary> The real, underlying type. </summary>
        public static readonly Type Type;

        /// <summary> True if this type requires the Marshaler to map variables. (No direct pointer dereferencing) </summary>
        public static readonly bool TypeRequiresMarshal;

        internal static readonly GetUnsafePtrDelegate GetUnsafePtr;

        static SizeCache()
        {
            Type = typeof(T);
            // Bools = 1 char.
            if (typeof(T) == typeof(bool))
            {
                UnmanagedSize = 1; // Or 8? Bits arent optimized by default
                Size = 1;
            }
            else if (typeof(T).IsEnum)
            {
                Type = Type.GetEnumUnderlyingType();
                UnmanagedSize = Size = Type.GetBinarySize();
            }
            else
                Size = Type.GetBinarySize();

            if (typeof(T) == typeof(string))
                UnmanagedSize = IntPtr.Size;

            TypeRequiresMarshal = Type.IsRequiringMarshalling();

            // Generate a method to get the address of a generic type. We'll be using this for RtlMoveMemory later for much faster structure reads.
            var method = new DynamicMethod(string.Format("GetPinnedPtr<{0}>", typeof(T).FullName.Replace(".", "<>")),
                typeof(void*),
                new[] { typeof(T).MakeByRefType() },
                typeof(SizeCache<>).Module);

            var generator = method.GetILGenerator();

            // ldarg 0
            generator.Emit(OpCodes.Ldarg_0);
            // (IntPtr)arg0
            generator.Emit(OpCodes.Conv_U);
            // ret arg0
            generator.Emit(OpCodes.Ret);
            GetUnsafePtr = (GetUnsafePtrDelegate)method.CreateDelegate(typeof(GetUnsafePtrDelegate));
        }

        #region Nested type: GetUnsafePtrDelegate

        internal unsafe delegate void* GetUnsafePtrDelegate(ref T value);

        #endregion
    }
}
