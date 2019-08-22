using System;
using System.IO;
using System.Reflection;
using BenchmarkDotNet.Running;
using DBClientFiles.NET.Benchmark.Misc;
using DBClientFiles.NET.Collections.Generic;

namespace DBClientFiles.NET.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summaries = BenchmarkSwitcher.FromTypes(new[] {
                typeof(WDBC.Benchmark),

                typeof(Misc.ByteArrayToStrings)
                // typeof(LanguageFeatureBenchmarks)
            }).Run(args);
        }
    }
}
