using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace DBClientFiles.NET.Utils
{
    [SuppressUnmanagedCodeSecurity]
    internal static class BinaryReaderExtensions
    {
        /// <summary>
        ///     Reads a generic structure from the current stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="br"></param>
        /// <returns></returns>
        public static T ReadStruct<T>(this BinaryReader br)
            where T : struct
        {
            if (SizeCache<T>.TypeRequiresMarshal)
            {
                var ptr = Marshal.AllocHGlobal(SizeCache<T>.Size);
                Marshal.Copy(br.ReadBytes(SizeCache<T>.Size), 0, ptr, SizeCache<T>.Size);
                var mret = Marshal.PtrToStructure<T>(ptr);
                Marshal.FreeHGlobal(ptr);
                return mret;
            }

            Span<byte> dataBytes = br.ReadBytes(SizeCache<T>.Size);
            return MemoryMarshal.Read<T>(dataBytes);
        }
    }
}
