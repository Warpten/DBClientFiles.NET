using System;

namespace DBClientFiles.NET.Utils
{
    public static unsafe class MiscExtensions
    {
        public static T To<T>(this byte[] self, long offset = 0) where T : struct
        {
            fixed (byte* data = self)
                return FastStructure<T>.PtrToStructure(new IntPtr(data + offset));
        }
    }
}
