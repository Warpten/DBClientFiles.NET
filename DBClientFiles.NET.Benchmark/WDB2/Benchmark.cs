using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Types.WDB2;

namespace DBClientFiles.NET.Benchmark.WDB2
{
    [CoreJob, DisplayName("WDB2")]
    public class Benchmark
    {
        [Benchmark(Description = "Item-sparse.db2")]
        public StorageList<ItemSparse> ItemSparse()
        {
            using (var fs = File.OpenRead(@"D:\Games\World of Warcraft 4.3.4 - Akama\dbc\Item-sparse.db2"))
                return new StorageList<ItemSparse>(StorageOptions.Default, fs);
        }
    }
}
