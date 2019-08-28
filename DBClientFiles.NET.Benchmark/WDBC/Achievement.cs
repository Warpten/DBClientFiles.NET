using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DBClientFiles.NET.Collections.Generic;

namespace DBClientFiles.NET.Benchmark.WDBC
{
    public class WDBC_Achievement : AbstractBenchmark
    {
        public WDBC_Achievement() : base(@"D:\Games\World of Warcraft 3.3.5\dbc\Achievement.dbc")
        {

        }

        [Benchmark]
        public void Achievement_Enumerator_Take1_WDBC()
        {
            File.Position = 0;
            new StorageEnumerable<Types.WDBC.Achievement>(StorageOptions.Default, File).Take(1).Consume(Consumer);
        }

        [Benchmark]
        public StorageList<Types.WDBC.Achievement> Achievement_List_WDBC()
        {
            File.Position = 0;
            return new StorageList<Types.WDBC.Achievement>(StorageOptions.Default, File);
        }
    }
}
