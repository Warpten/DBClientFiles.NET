using System;

namespace DBClientFiles.NET.Definitions.Utils
{
    public static class StringExtensions
    {
        public static Type ToType(this string input)
        {
            if (input == "int") return typeof(int);
            if (input == "uint") return typeof(uint);
            if (input == "float") return typeof(float);
            if (input == "locstring") return typeof(string);

            return null;
        }

        public static Type AdjustBitCount(this Type type, int bitCount)
        {
            if (type == typeof(float)) return type;

            if (bitCount == 64)
                return type.IsSigned() ? typeof(long) : typeof(ulong);
            if (bitCount == 32)
                return type.IsSigned() ? typeof(int) : typeof(uint);
            if (bitCount == 16)
                return type.IsSigned() ? typeof(short) : typeof(ushort);
            if (bitCount == 8)
                return type.IsSigned() ? typeof(sbyte) : typeof(byte);

            return type;
        }

        public static bool IsSigned(this Type type)
        {
            if (type == typeof(long)) return true;
            if (type == typeof(int)) return true;
            if (type == typeof(short)) return true;
            if (type == typeof(sbyte)) return true;
            if (type == typeof(float)) return true;

            return false;
        }

        public static Type InverseSignedness(this Type type)
        {
            if (type == typeof(long)) return typeof(ulong);
            if (type == typeof(ulong)) return typeof(long);

            if (type == typeof(int)) return typeof(uint);
            if (type == typeof(uint)) return typeof(int);
            if (type == typeof(short)) return typeof(ushort);
            if (type == typeof(ushort)) return typeof(short);
            if (type == typeof(sbyte)) return typeof(byte);
            if (type == typeof(byte)) return typeof(sbyte);

            if (type == typeof(float)) return type;
            return null;
        }
    }
}
