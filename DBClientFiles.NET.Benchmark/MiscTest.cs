using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Benchmark.Attributes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace DBClientFiles.NET.Benchmark
{
    /*[NetCoreJob]
    [DisassemblyDiagnoser(printAsm: true, printSource: true, recursiveDepth: 3)]
    public class MiscTest
    {
        private byte[] data;

        private const int OFFSET = 7;
        private const int COUNT = 5;

        [GlobalSetup]
        public void GlobalSetup()
        {
            data = new byte[60];
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [SuppressUnmanagedCodeSecurity]
        [Benchmark(Description = "ReadArray - CopyBlock", Baseline = true)]
        public unsafe int[] BlockCopy()
        {
            return BlockCopyArrayRead<int>(OFFSET, COUNT);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [SuppressUnmanagedCodeSecurity]
        [Benchmark(Description = "ReadArray - manual loop over pointer")]
        public unsafe int[] PointerLoopCopy()
        {
            return PointerLoopCopyT<int>(OFFSET, COUNT);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [SuppressUnmanagedCodeSecurity]
        [Benchmark(Description = "ReadArray - loop of AsPointer")]
        public unsafe int[] AsPointerLoop()
        {
            return AsPointerLoopT<int>(OFFSET, COUNT);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T[] AsPointerLoopT<T>(int offset, int count) where T : unmanaged
        {
            var array = new T[count];
            for (var i = 0; i < count; ++i)
                array[i] = *(T*)Unsafe.AsPointer(ref data[offset + i * Unsafe.SizeOf<T>()]);

            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T[] BlockCopyArrayRead<T>(int offset, int count) where T : unmanaged
        {
            var dataPtr = Unsafe.AsPointer(ref data[offset]);
            var array = new T[count];

            Unsafe.CopyBlock(Unsafe.AsPointer(ref array[0]), dataPtr, (uint)(count * Unsafe.SizeOf<T>()));
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T[] PointerLoopCopyT<T>(int offset, int count) where T : unmanaged
        {
            var dataPtr = (T*)Unsafe.AsPointer(ref data[offset]);
            var array = new T[count];
            for (var i = 0; i < COUNT; ++i)
                array[i] = dataPtr[i];

            return array;
        }
    }*/
}
