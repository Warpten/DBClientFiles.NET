using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Collections.Generic;
using System.Linq;

namespace DBClientFiles.NET.Benchmark.WDC1
{
    public class Timing : AbstractBenchmark
    {
        public Timing() : base(@"D:\Games\Achievement.25928.db2")
        {

        }

        [Benchmark(Description= "WDC1.Achievement - First")]
        public Types.WDC1.Achievement First()
        {
            File.Position = 0;
            return new StorageEnumerable<Types.WDC1.Achievement>(StorageOptions.Default, File).First();
        }

        [Benchmark(Description = "WDC1.Achievement - All")]
        public StorageList<Types.WDC1.Achievement> All()
        {
            File.Position = 0;
            return new StorageList<Types.WDC1.Achievement>(StorageOptions.Default, File);
        }
    }
}
