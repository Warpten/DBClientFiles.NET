using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Benchmark.Attributes;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Benchmark
{
    [NetCoreJob]
    public class SpanTest
    {
        [Benchmark(Description = "stackalloc", Baseline = true)]
        public int SpanSmall()
        {
            Span<byte> smallData = stackalloc byte[20];
            return MemoryMarshal.Read<int>(smallData);
        }

        [Benchmark(Description = "new byte[]")]
        public unsafe int FastStructureSmall()
        {
            Span<byte> smallData = new byte[20];
            return MemoryMarshal.Read<int>(smallData);
        }
    }
}
