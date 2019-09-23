using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace DBClientFiles.NET.Benchmark.WDB2.ItemSparse
{
    [MemoryDiagnoser]
    public class Allocation : AbstractAllocationBenchmark<Types.WDB2.ItemSparse>
    {
        public Allocation() : base(@"D:\Games\World of Warcraft 4.3.4 - Akama\dbc\Item-sparse.db2")
        {

        }

        [Benchmark(Description = "WDB2.ItemSparse - One")]
        public Types.WDB2.ItemSparse AllocationsForOne()
        {
            Enumerable.MoveNext();
            return Enumerable.Current;
        }

        [Benchmark(Description = "new WDB2.ItemSparse()", Baseline = true)]
        public Types.WDB2.ItemSparse AllocationForT()
        {
            return new Types.WDB2.ItemSparse();
        }
    }
}
