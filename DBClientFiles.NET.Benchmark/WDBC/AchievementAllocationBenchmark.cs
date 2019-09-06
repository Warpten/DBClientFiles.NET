using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Types.WDBC;

namespace DBClientFiles.NET.Benchmark.WDBC
{
    [MemoryDiagnoser]
    public class AchievementAllocationBenchmark : AbstractBenchmark
    {
        private IEnumerator<Achievement> _enumerable;

        public AchievementAllocationBenchmark() : base(@"D:\Games\World of Warcraft 3.3.5\dbc\Achievement.dbc")
        {

        }

        [IterationSetup]
        public void IterationSetup()
        {
            File.Position = 0;
            _enumerable = new StorageEnumerable<Achievement>(StorageOptions.Default, File).GetEnumerator();
            _enumerable.MoveNext();
        }

        [Benchmark]
        public Achievement AllocationsForOne()
        {
            _enumerable.MoveNext();
            return _enumerable.Current;
        }

        [Benchmark(Baseline = true)]
        public Achievement AllocationForT()
        {
            return new Achievement();
        }
    }
}
