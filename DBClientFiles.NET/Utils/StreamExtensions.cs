using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Utils
{
    internal static class StreamExtensions
    {
        public static unsafe T Read<T>(this Stream stream) where T : struct
        {
            Span<byte> instance = stackalloc byte[Unsafe.SizeOf<T>()];
            stream.Read(instance);
            return Unsafe.AsRef<T>(Unsafe.AsPointer(ref instance[0]));
        }

        public static unsafe T[] ReadArray<T>(this Stream stream, int count) where T : struct
        {
            var data = new T[count];
            var dataPtr = MemoryMarshal.AsBytes(new Span<T>(data));
            stream.Read(dataPtr);

            return data;
        }
    }
}
