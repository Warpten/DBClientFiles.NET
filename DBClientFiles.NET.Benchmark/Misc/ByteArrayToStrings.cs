using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Benchmark.Attributes;

namespace DBClientFiles.NET.Benchmark.Misc
{
    [NetCoreJob]
    public class ByteArrayToStrings
    {
        private byte[] data = new byte[]
        {
            0x4E, 0x6F, 0x74, 0x68, 0x69, 0x6E, 0x67, 0x20, 0x42, 0x6F, 0x72, 0x69, 0x6E, 0x67, 0x20,
            0x41, 0x62, 0x6F, 0x75, 0x74, 0x20, 0x42, 0x6F, 0x72, 0x65, 0x61, 0x6E, 0x00, 0x43, 0x6F, 0x6D,
            0x70, 0x6C, 0x65, 0x74, 0x65, 0x20, 0x31, 0x33, 0x30, 0x20, 0x71, 0x75, 0x65, 0x73, 0x74, 0x73,
            0x20, 0x69, 0x6E, 0x20, 0x42, 0x6F, 0x72, 0x65, 0x61, 0x6E, 0x20, 0x54, 0x75, 0x6E, 0x64, 0x72,
            0x61, 0x2E, 0x00
        };

        [Benchmark]
        public List<string> Naive()
        {
            int cursor = 0;
            int length = data.Length;

            var container = new List<string>();

            while (cursor != length)
            {
                var stringStart = cursor;
                while (data[cursor] != 0)
                    ++cursor;

                if (cursor - stringStart > 1)
                {
                    var value = Encoding.UTF8.GetString(data, stringStart, cursor - stringStart);

                    container.Add(value);
                }

                cursor += 1;
            }

            return container;
        }

        [Benchmark]
        public unsafe List<String> Unroll4()
        {
            var wordBuffer = (int*)Unsafe.AsPointer(ref data[0]);
            int wordCount = 0;

            var container = new List<string>();

            var startOffsetStr = 0;
            while (true)
            {
                int mask = wordBuffer[wordCount];
                while (((mask - 0x01010101) & ~mask & 0x80808080) == 0)
                {
                    ++wordCount;
                    mask = wordBuffer[wordCount];
                }

                var trailingCount = 0;
                for (var i = 0; i < 4; ++i, ++trailingCount)
                    if (((mask >> (8 * i)) & 0xFF) == 0x00)
                        break;

                var strLength = 4 * wordCount + trailingCount;
                if (strLength > 0)
                {
                    container.Add(Encoding.UTF8.GetString(data, startOffsetStr, strLength));
                    startOffsetStr += strLength + 1;
                }
                else
                    startOffsetStr += 1;
                
                if (startOffsetStr == data.Length)
                    break;

                wordBuffer = (int*)Unsafe.AsPointer(ref data[startOffsetStr]);
                wordCount = 0;
            }

            return container;
        }

        [Benchmark]
        public unsafe List<String> Unroll8()
        {
            var wordBuffer = (ulong*)Unsafe.AsPointer(ref data[0]);
            int wordCount = 0;

            var container = new List<string>();

            var startOffsetStr = 0;
            while (true)
            {
                ulong mask = wordBuffer[wordCount];
                while (((mask - 0x0101010101010101uL) & ~mask & 0x8080808080808080uL) == 0)
                {
                    ++wordCount;
                    mask = wordBuffer[wordCount];
                }

                var trailingCount = 0;
                for (var i = 0; i < 8; ++i, ++trailingCount)
                    if (((mask >> (8 * i)) & 0xFF) == 0x00)
                        break;

                var strLength = 8 * wordCount + trailingCount;
                if (strLength > 0)
                {
                    container.Add(Encoding.UTF8.GetString(data, startOffsetStr, strLength));
                    startOffsetStr += strLength + 1;
                }
                else
                    startOffsetStr += 1;
                
                if (startOffsetStr == data.Length)
                    break;

                wordBuffer = (ulong*)Unsafe.AsPointer(ref data[startOffsetStr]);
                wordCount = 0;
            }

            return container;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void WidenSIMD(char* dst, byte* src, int c)
        {
            for (; c > 0 && ((long)dst & 0xF) != 0; c--)
                *dst++ = (char)*src++;
            
            for (; (c -= 0x10) >= 0; src += 0x10, dst += 0x10)
                Vector.Widen(Unsafe.AsRef<Vector<byte>>(src),
                    out Unsafe.AsRef<Vector<ushort>>(dst + 0),
                    out Unsafe.AsRef<Vector<ushort>>(dst + 0x10));

            for (c += 0x10; c > 0; c--)
                *dst++ = (char)*src++;
        }

        [Benchmark]
        public unsafe List<string> VectorizedUnrolled4()
        {
            var container = new List<string>();

            Span<char> dst = new char[data.Length];

            var pdst = (char*) Unsafe.AsPointer(ref dst[0]);
            var psrc = (byte*) Unsafe.AsPointer(ref data[0]);

            WidenSIMD(pdst, psrc, data.Length);

            var pStart = (int*) pdst;
            var strOffset = 0;
            while (true)
            {
                var pEnd = pStart;
                while (((*pEnd - 0x01010101) & (~(*pEnd) & 0x80808080)) == 0x80008000)
                    ++pEnd;

                var trailingBytes = 0;
                for (var i = 0; i < 4; i += 2, trailingBytes += 2)
                    if ((((*pEnd) >> (8 * i)) & 0xFF) == 0)
                        break;

                var chunkSize = ((pEnd - pStart) * 4 + 4 - trailingBytes) / 2;
                if (chunkSize > 0)
                    container.Add(new string(pdst, 0, (int) chunkSize));

                strOffset += (int) chunkSize + 1;
                if (strOffset >= data.Length)
                    break;

                pdst += chunkSize + 1;
                pStart = (int*) pdst;
            }

            return container;
        }

        [Benchmark(Baseline = true)]
        public unsafe List<string> Vectorized()
        {
            var container = new List<string>();

            Span<char> dst = new char[data.Length];

            var pdst = (char*)Unsafe.AsPointer(ref dst[0]);
            var psrc = (byte*)Unsafe.AsPointer(ref data[0]);

            WidenSIMD(pdst, psrc, data.Length);

            var cursor = 0;
            while (cursor != dst.Length)
            {
                int end = cursor;
                while (dst[end] != 0x00)
                    ++end;

                container.Add(new string(pdst, cursor, end - cursor));
                cursor = end + 1;
            }

            return container;
        }
    }
}
