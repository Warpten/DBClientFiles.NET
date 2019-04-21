using System.IO;
using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Benchmark.Attributes;
using DBClientFiles.NET.Benchmark.Structures.DBC;
using DBClientFiles.NET.Collections.Generic;

namespace DBClientFiles.NET.Benchmark.Benchmarks
{
    [NetCoreJob]
    public class ComparativeBenchmark
    {
        [Benchmark(Description = "DBClientFiles.NET")]
        public StorageList<AreaTriggerEntry> TestDBFilesClientNET()
        {
            using (var fs = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\AreaTrigger.dbc"))
                return new StorageList<AreaTriggerEntry>(in StorageOptions.Default, fs);
        }
    }
}
