using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Collections.Generic;
using System.Collections.Generic;
using System.Linq;

namespace DBClientFiles.NET.Benchmark.WDB2.ItemSparse
{
    public class Timing : AbstractBenchmark
    {
        public Timing() : base(@"D:\Games\World of Warcraft 4.3.4 - Akama\dbc\Item-sparse.db2")
        {

        }

        [Benchmark]
        public StorageList<Types.WDB2.ItemSparse> ItemSparse_List()
        {
            File.Position = 0;
            return new StorageList<Types.WDB2.ItemSparse>(StorageOptions.Default, File);
        }

        [Benchmark]
        public Types.WDB2.ItemSparse ItemSparse_First()
        {
            File.Position = 0;
            return new StorageEnumerable<Types.WDB2.ItemSparse>(StorageOptions.Default, File).First();
        }
    }
}
