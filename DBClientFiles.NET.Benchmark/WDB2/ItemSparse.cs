using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DBClientFiles.NET.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DBClientFiles.NET.Benchmark
{
    [CategoriesColumn, AnyCategoriesFilter("WDB2")]

    public class WDB2_ItemSparse : AbstractBenchmark
    {
        public WDB2_ItemSparse() : base(@"D:\Games\World of Warcraft 4.3.4 - Akama\dbc\Item-sparse.db2")
        {

        }

        [Benchmark(Description = "Item-sparse (take all)")]
        [BenchmarkCategory("Item-sparse.db2", "WDB2")]
        public StorageList<Types.WDB2.ItemSparse> ItemSparse_List()
        {
            return new StorageList<Types.WDB2.ItemSparse>(StorageOptions.Default, File);
        }

        [Benchmark(Description = "Item-sparse (take all)")]
        [BenchmarkCategory("Item-sparse.db2", "WDB2")]
        public void ItemSparse_Take1()
        {
            new StorageEnumerable<Types.WDB2.ItemSparse>(StorageOptions.Default, File).Take(1).Consume(Consumer);
        }
    }
}
