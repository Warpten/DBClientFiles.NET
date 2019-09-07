using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DBClientFiles.NET.Collections.Generic;
using System.Collections.Generic;
using System.Linq;

namespace DBClientFiles.NET.Benchmark.WDB2
{
    public class WDB2_ItemSparse : AbstractBenchmark
    {
        public WDB2_ItemSparse() : base(@"D:\Games\World of Warcraft 4.3.4 - Akama\dbc\Item-sparse.db2")
        {

        }

        [Params(5, 100, 500, 1000, 5000, 10000)] public int SkipCount;

        // [Benchmark]
        public StorageList<Types.WDB2.ItemSparse> ItemSparse_List()
        {
            File.Position = 0;
            return new StorageList<Types.WDB2.ItemSparse>(StorageOptions.Default, File);
        }

        // [Benchmark]
        public Types.WDB2.ItemSparse ItemSparse_First()
        {
            File.Position = 0;
            return new StorageEnumerable<Types.WDB2.ItemSparse>(StorageOptions.Default, File).First();
        }

        [Benchmark]
        public Types.WDB2.ItemSparse ItemSparse_OptimizedSkip()
        {
            File.Position = 0;
            return new StorageEnumerable<Types.WDB2.ItemSparse>(StorageOptions.Default, File).Skip(SkipCount).First();
        }


        [Benchmark]
        public Types.WDB2.ItemSparse ItemSparse_RegularSkip()
        {
            File.Position = 0;
            IEnumerable<Types.WDB2.ItemSparse> enumerable = new StorageEnumerable<Types.WDB2.ItemSparse>(StorageOptions.Default, File);
            return enumerable.Skip(SkipCount).First();
        }
    }
}
