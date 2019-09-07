using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DBClientFiles.NET.Collections.Generic;
using System.Linq;

namespace DBClientFiles.NET.Benchmark.WDB2
{
    public class WDB2_ItemSparse : AbstractBenchmark
    {
        public WDB2_ItemSparse() : base(@"D:\Games\World of Warcraft 4.3.4 - Akama\dbc\Item-sparse.db2")
        {

        }

        [Benchmark]
        public StorageList<Types.WDB2.ItemSparse> ItemSparse_List()
        {
            File.Position = 0;
            return new StorageList<Types.WDB2.ItemSparse>(StorageOptions.Default, File);
        }

        [Benchmark]
        public void ItemSparse_Take1()
        {
            File.Position = 0;
            new StorageEnumerable<Types.WDB2.ItemSparse>(StorageOptions.Default, File).Take(1).Consume(Consumer);
        }
    }
}
