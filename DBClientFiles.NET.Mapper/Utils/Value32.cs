using System;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Mapper.Utils
{
    // All the Equals implementation treat 0 as a false positive and move on

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
}
