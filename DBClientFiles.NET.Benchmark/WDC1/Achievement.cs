using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DBClientFiles.NET.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBClientFiles.NET.Benchmark.WDC1
{
    public class WDC1_Achievement : AbstractBenchmark
    {
        public WDC1_Achievement() : base(@"D:\Games\Achievement.25928.db2")
        {

        }

        [Benchmark]
        public void Achievement_Enumerator_Take1_WDC1()
        {
            File.Position = 0;
            new StorageEnumerable<Types.WDC1.Achievement>(StorageOptions.Default, File).Take(1).Consume(Consumer);
        }

        [Benchmark]
        public StorageList<Types.WDC1.Achievement> Achievement_List_WDC1()
        {
            File.Position = 0;
            return new StorageList<Types.WDC1.Achievement>(StorageOptions.Default, File);
        }
    }
}
