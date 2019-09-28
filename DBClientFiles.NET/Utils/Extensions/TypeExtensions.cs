using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Parsing.Reflection;

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

        public static TypeTokenType ToTypeToken(this MemberTypes type)
        {
            return type switch
            {
                MemberTypes.Field => TypeTokenType.Field,
                MemberTypes.Property => TypeTokenType.Property,

                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }
    }
}
