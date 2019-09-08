using BenchmarkDotNet.Attributes;

namespace DBClientFiles.NET.Benchmark.WDBC.Achievement
{
    [MemoryDiagnoser]
    public class Allocation : AbstractAllocationBenchmark<Types.WDBC.Achievement>
    {
        public Allocation() : base(@"D:\Games\World of Warcraft 3.3.5\dbc\Achievement.dbc")
        {

        }

        [Benchmark(Description = "WDBC.Achievement - One")]
        public Types.WDBC.Achievement AllocationsForOne()
        {
            Enumerable.MoveNext();
            return Enumerable.Current;
        }

        [Benchmark(Description = "new WDBC.Achievement()", Baseline = true)]
        public Types.WDBC.Achievement AllocationForT()
        {
            return new Types.WDBC.Achievement();
        }
    }
}
