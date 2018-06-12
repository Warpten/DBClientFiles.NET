using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Mapper.Utils
{
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