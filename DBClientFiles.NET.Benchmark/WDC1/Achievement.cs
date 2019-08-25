using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DBClientFiles.NET.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBClientFiles.NET.Benchmark.WDC1
{
    [CategoriesColumn, AnyCategoriesFilter("WDC1")]
    public class WDC1_Achievement : AbstractBenchmark
    {
        public WDC1_Achievement() : base(@"D:\Games\Achievement.25928.db2")
        {

        }

        [Benchmark(Description = "Achievement (take 1)")]
        [BenchmarkCategory("WDBC")]
        public void Achievement_Enumerator_Take1()
        {
            new StorageEnumerable<Types.WDC1.Achievement>(StorageOptions.Default, File).Take(1).Consume(Consumer);
        }

        [Benchmark(Description = "Achievement (take all)")]
        [BenchmarkCategory("WDBC")]
        public StorageList<Types.WDC1.Achievement> Achievement_List()
        {
            return new StorageList<Types.WDC1.Achievement>(StorageOptions.Default, File);
        }
    }
}
