using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Parsing.Reflection;

using InlineIL;

namespace DBClientFiles.NET.Utils.Extensions
{
    internal static class TypeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasDefaultConstructor(this Type t)
        {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool IsSigned(this Type t)
        {
            if (t == typeof(long))
                return true;

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

        public static TypeTokenKind ToTypeToken(this MemberTypes type)
        {
            return type switch
            {
                MemberTypes.Field => TypeTokenKind.Field,
                MemberTypes.Property => TypeTokenKind.Property,

                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }

        /// <summary>
        /// Returns the size of this type, assuming it's a value type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Size(this Type type)
        {
            Debug.Assert(type.IsValueType);

            IL.Emit.Sizeof(TypeRef.Type(type));
            return IL.Return<int>();
        }

        /// <summary>
        /// Determines wether this type represents a value tuple.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValueTuple(this Type type)
        {
            if (!type.IsGenericType || !type.IsValueType)
                return false;

            return type.GetGenericTypeDefinition().FullName.StartsWith("System.ValueTuple");
        }
    }
}
