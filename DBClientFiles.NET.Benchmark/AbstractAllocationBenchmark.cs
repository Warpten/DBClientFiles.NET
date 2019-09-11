using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", Justification = "Benchmark, irrelevant + FP")]
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
