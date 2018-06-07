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
            var asSpan = b.AsSpan();
            return MemoryMarshal.Read<int>(asSpan);
        }

        [Benchmark(Description = "union")]
        public int FastStructureSmall()
        {
            return u.i;
        }
        
        private _union u;
        private byte[] b;

        [GlobalSetup]
        public void setup()
        {
            u = new _union {u = 0xDEADBEEF};
            b = new byte[] {0xEF, 0xBE, 0xAD, 0xDE};
        }

        [StructLayout(LayoutKind.Explicit)]
        unsafe struct _union
        {
            [FieldOffset(0)] public int i;
            [FieldOffset(0)] public float f;
            [FieldOffset(0)] public fixed byte b[4];
            [FieldOffset(0)] public uint u;
        }
    }
}
