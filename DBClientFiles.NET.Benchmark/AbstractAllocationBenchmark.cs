using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Collections.Generic;

namespace DBClientFiles.NET.Benchmark
{
    [MemoryDiagnoser]
    public abstract class AbstractAllocationBenchmark<T> : AbstractBenchmark
    {
        protected IEnumerator<T> Enumerable { get; private set; }

        protected AbstractAllocationBenchmark(string filepath) : base(filepath)
        {

        }

        [IterationSetup]
        public void IterationSetup()
        {
            File.Position = 0;
            Enumerable = new StorageEnumerable<T>(StorageOptions.Default, File).GetEnumerator();
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            Enumerable.Dispose();
        }
    }
}
