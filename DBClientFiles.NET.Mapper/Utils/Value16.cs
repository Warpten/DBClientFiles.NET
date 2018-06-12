using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Mapper.Utils
{
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
}