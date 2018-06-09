using System;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.AutoMapper
{
    // All the Equals implementation treat 0 as a false positive and move on

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct Value64
    {
        [FieldOffset(0)] public ulong UInt64;
        [FieldOffset(0)] public long Int64;

        public override bool Equals(object obj)
        {
            if (obj is long i)
                return i == 0 || Int64 == i;

            if (obj is ulong u)
                return u == 0u || UInt64 == u;

            return false;
        }

        public override int GetHashCode()
        {
            return (int)(Int64 & 0xFFFFFFFF);
        }

        public static implicit operator Value64(long value)  => new Value64 { Int64 = value };
        public static implicit operator Value64(ulong value) => new Value64 { UInt64 = value };
    }

    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Value32
    {
        [FieldOffset(0)] public uint UInt32;
        [FieldOffset(0)] public int Int32;
        [FieldOffset(0)] public float Single;

        public override bool Equals(object obj)
        {
            if (obj is int i)
                return i == 0 || Int32 == i;

            if (obj is float f)
                return Math.Abs(f) < 1.0E-5f || Math.Abs(Single - f) < 1.0E-5f;

            if (obj is uint u)
                return u == 0u || UInt32 == u;

            return false;
        }

        public override int GetHashCode()
        {
            return Int32;
        }

        public static implicit operator Value32(int value)   => new Value32 { Int32 = value };
        public static implicit operator Value32(uint value)  => new Value32 { UInt32 = value };
        public static implicit operator Value32(float value) => new Value32 { Single = value };
    }

    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public struct Value16
    {
        [FieldOffset(0)] public ushort UInt16;
        [FieldOffset(0)] public short Int16;

        public override bool Equals(object obj)
        {
            if (obj is short i)
                return i == 0 || Int16 == i;

            if (obj is ushort u)
                return u == 0u || UInt16 == u;

            return false;
        }

        public override int GetHashCode()
        {
            return Int16;
        }

        public static implicit operator Value16(short value) => new Value16 { Int16 = value };
        public static implicit operator Value16(ushort value) => new Value16 { UInt16 = value };
    }

    [StructLayout(LayoutKind.Explicit, Size = 1)]
    public struct Value8
    {
        [FieldOffset(0)] public byte UInt8;
        [FieldOffset(0)] public sbyte Int8;

        public override bool Equals(object obj)
        {
            if (obj is sbyte i)
                return i == 0 || Int8 == i;

            if (obj is byte u)
                return u == 0u || UInt8 == u;

            return false;
        }

        public override int GetHashCode()
        {
            return Int8;
        }

        public static implicit operator Value8(sbyte value) => new Value8 { Int8 = value };
        public static implicit operator Value8(byte value)  => new Value8 { UInt8 = value };
    }
}
