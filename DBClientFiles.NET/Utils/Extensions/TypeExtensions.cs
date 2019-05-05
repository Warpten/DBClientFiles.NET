using System;
using System.Reflection;
using DBClientFiles.NET.Parsing.Reflection;

namespace DBClientFiles.NET.Utils.Extensions
{
    internal static class TypeExtensions
    {
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
            switch (type)
            {
                case MemberTypes.Field:
                    return TypeTokenType.Field;
                case MemberTypes.Property:
                    return TypeTokenType.Property;
            }

            throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
