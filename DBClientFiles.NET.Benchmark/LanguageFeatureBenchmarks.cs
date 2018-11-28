using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Benchmark.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace DBClientFiles.NET.Benchmark
{
    [NetCoreJob]
    [DisassemblyDiagnoser(printAsm: true, printSource: true, recursiveDepth: 1)]
    public unsafe class LanguageFeatureBenchmarks
    {
        private byte[] _stagingBuffer;

        [GlobalSetup]
        public void Setup()
        {
            _stagingBuffer = new byte[60];
            for (var i = 0; i < _stagingBuffer.Length; ++i)
                _stagingBuffer[i] = (byte) i;
        }

        [Benchmark]
        public int TestRead()
        {
            return Read<int>(0);
        }

        [Benchmark]
        public int TestReadRef()
        {
            return ReadRef<int>(0);
        }

        [Benchmark]
        public int TestReadRefReadonly()
        {
            return ReadRefReadonly<int>(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T Read<T>(int cursor) where T : unmanaged
        {
            var value = *(T*)Unsafe.AsPointer(ref _stagingBuffer[cursor]);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T ReadRef<T>(int cursor) where T : unmanaged
        {
            ref var value = ref *(T*)Unsafe.AsPointer(ref _stagingBuffer[cursor]);
            return ref value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref readonly T ReadRefReadonly<T>(int cursor) where T : unmanaged
        {
            ref var value = ref *(T*)Unsafe.AsPointer(ref _stagingBuffer[cursor]);
            return ref value;
        }
    }
}
