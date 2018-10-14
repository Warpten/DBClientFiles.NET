using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Utils
{
    internal static class UnsafeCache<T>
    {
        public static int Size { get; private set; }
        public static int BitSize { get; private set; }
        public static Type Type { get; } = typeof(T);

        static UnsafeCache()
        {
            if (typeof(T).IsValueType)
                Size = Unsafe.SizeOf<T>();
            else
                Size = UnsafeCache.SizeOf(typeof(T));
            BitSize = Size * 8;
        }
    }

    internal static class UnsafeCache
    {
        private static Dictionary<Type, int> _sizes = new Dictionary<Type, int>();

        private static MethodInfo _SizeOf = typeof(Unsafe).GetMethod("SizeOf", Type.EmptyTypes);

        public static int SizeOf(Type type)
        {
            if (_sizes.TryGetValue(type, out var size))
                return size;

            if (type.IsValueType)
            {
                var methodInfo = _SizeOf.MakeGenericMethod(type);

                var lambda = Expression.Lambda<Func<int>>(Expression.Call(methodInfo)).Compile();
                size = _sizes[type] = lambda();
                return size;
            }
            else
            {
                try {
                    size = _sizes[type] = Marshal.SizeOf(type);
                } catch {
                    size = 0;

                    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        var attr = field.GetCustomAttribute<FixedBufferAttribute>(false);
                        if (attr != null)
                            size += SizeOf(attr.ElementType) * attr.Length;
                        else
                            size += SizeOf(field.FieldType);
                    }
                    _sizes[type] = size;
                }
                return size;
            }
        }

        public static int BitSizeOf(Type t)
            => SizeOf(t) * 8;
    }
}
