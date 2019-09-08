using System.ComponentModel;
using System.Linq;
using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Collections.Generic;

namespace DBClientFiles.NET.Benchmark.WDBC.Achievement
{
    public class Timing : AbstractBenchmark
    {
        public Timing() : base(@"D:\Games\World of Warcraft 3.3.5\dbc\Achievement.dbc")
        {

        }

        [Benchmark(Description = "WDBC.Achievement - First")]
        public Types.WDBC.Achievement First()
        {
            File.Position = 0;
            return new StorageEnumerable<Types.WDBC.Achievement>(StorageOptions.Default, File).First();
        }

        [Benchmark(Description = "WDBC.Achievement - All")]
        public StorageList<Types.WDBC.Achievement> All()
        {
            File.Position = 0;
            return new StorageList<Types.WDBC.Achievement>(StorageOptions.Default, File);
        }
    }
}
