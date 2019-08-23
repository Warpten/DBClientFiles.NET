using System.IO;
using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Collections.Generic;

namespace DBClientFiles.NET.Benchmark
{
    [CategoriesColumn, AnyCategoriesFilter("WDBC", "WDB2")]
    public class Benchmark
    {
        [Benchmark(Description = "Item-sparse.db2")]
        [BenchmarkCategory("WDB2")]
        public StorageList<Types.WDB2.ItemSparse> ItemSparse()
        {
            using (var fs = File.OpenRead(@"D:\Games\World of Warcraft 4.3.4 - Akama\dbc\Item-sparse.db2"))
                return new StorageList<Types.WDB2.ItemSparse>(StorageOptions.Default, fs);
        }

        [Benchmark(Description = "Achievement.dbc")]
        [BenchmarkCategory("WDBC")]
        public StorageList<Types.WDBC.Achievement> AchievementWDBC()
        {
            using (var fs = File.OpenRead(@"D:\Games\World of Warcraft 3.3.5\dbc\Achievement.dbc"))
                return new StorageList<Types.WDBC.Achievement>(StorageOptions.Default, fs);
        }
    }
}
