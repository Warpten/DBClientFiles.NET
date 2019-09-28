using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Collections.Generic;

namespace DBClientFiles.NET.Benchmark.WDB2.ItemSparse
{
    public class Skip : AbstractBenchmark
    {
        public Skip() : base(@"D:\Games\World of Warcraft 4.3.4 - Akama\dbc\Item-sparse.db2")
        {

        }

        [Params(5, 100, 500, 1000, 5000, 10000)] public int SkipCount;

        [Benchmark(Description = "WDB2.ItemSparse - Optimized Skip")]
        [SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", Justification = "Benchmark")]
        public Types.WDB2.ItemSparse ItemSparse_OptimizedSkip()
        {
            File.Position = 0;
            return new StorageEnumerable<Types.WDB2.ItemSparse>(StorageOptions.Default, File).Skip(SkipCount).First();
        }


        [Benchmark(Description = "WDB2.ItemSparse - LINQ Skip")]
        public Types.WDB2.ItemSparse ItemSparse_RegularSkip()
        {
            File.Position = 0;
            IEnumerable<Types.WDB2.ItemSparse> enumerable = new StorageEnumerable<Types.WDB2.ItemSparse>(StorageOptions.Default, File);
            return enumerable.Skip(SkipCount).First();
        }
    }
}
