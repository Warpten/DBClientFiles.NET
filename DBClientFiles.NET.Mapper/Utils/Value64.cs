using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Mapper.Utils
{
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
}