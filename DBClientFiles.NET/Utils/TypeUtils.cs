using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Utils
{
    /// <summary>
    /// Inspired by Apoc's SizeCache, but tailored to runtime type informations instead of compile-time.
    /// </summary>
    internal static class TypeUtils
    {
        private static Dictionary<Type, int> _typeSizes = new Dictionary<Type, int>();

        public static bool IsRequiringMarshalling(this Type t)
        {
            var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (var index = 0; index < fields.Length; index++)
            {
                var field = fields[index];
                var requires = field.GetCustomAttributes(typeof(MarshalAsAttribute), true).Length != 0;
                if (requires)
                    return true;

                if (t == typeof(IntPtr))
                    continue;

                if (Type.GetTypeCode(t) == TypeCode.Object)
                    requires |= field.FieldType.IsRequiringMarshalling();

                return requires;
            }

            return false;
        }

        public static int GetBinarySize(this Type t)
        {
            if (t.IsArray)
                return t.GetElementType().GetBinarySize();
            
            if (t == typeof(string))
                return 4;

            if (t == typeof(IntPtr))
                return 4;

            if (t.IsEnum)
                return t.GetEnumUnderlyingType().GetBinarySize();

            if (_typeSizes.TryGetValue(t, out var size))
                return size;

            try
            {
                // Try letting the marshaler handle getting the size.
                // It can *sometimes* do it correctly
                // If it can't, fall back to our own methods.
                // var o = Activator.CreateInstance(t);
                return _typeSizes[t] = Marshal.SizeOf(t);
            }
            catch
            {
                var totalSize = 0;
                var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                for (var index = 0; index < fields.Length; index++)
                {
                    var field = fields[index];
                    var fba = field.GetCustomAttribute<FixedBufferAttribute>(false);
                    if (fba != null)
                    {
                        totalSize += fba.ElementType.GetBinarySize() * fba.Length;
                        continue;
                    }

                    totalSize += field.FieldType.GetBinarySize();
                }

                return _typeSizes[t] = totalSize;
            }
        }

        public static bool IsSigned(this Type t)
        {
            if (t == typeof(int))
                return true;

            if (t == typeof(short))
                return true;

            if (t == typeof(sbyte))
                return true;

            if (t == typeof(float))
                return true;

            return false;
        }
    }
}
