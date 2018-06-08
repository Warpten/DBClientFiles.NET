using System.IO;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Benchmark.Attributes;
using DBClientFiles.NET.Collections.Generic;
using WDBC = DBClientFiles.NET.Data.WDBC;
using WDB2 = DBClientFiles.NET.Data.WDB2;
using WDC1 = DBClientFiles.NET.Data.WDC1;
using WDC2 = DBClientFiles.NET.Data.WDC2;

namespace DBClientFiles.NET.Benchmark
{
    [NetCoreJob]
    public class AchievementTest
    {
        private Stream OpenFile(string resourceName)
        {
            return File.OpenRead($@"C:\Users\Vincent Piquet\source\repos\DBClientFiles.NET\DBClientFiles.NET.Benchmark\Files\{resourceName}");
        }

        [Benchmark(Description = "Achievement (WDBC)", OperationsPerInvoke = 5)]
        public StorageList<WDBC.AchievementEntry> WDBC()
        {
            using (var fs = OpenFile("Achievement.WDBC.dbc"))
                return new StorageList<WDBC.AchievementEntry>(fs);
        }

        [Benchmark(Description = "Achievement (WDB2)", OperationsPerInvoke = 5)]
        public StorageList<WDB2.AchievementEntry> WDB2()
        {
            using (var fs = OpenFile("Achievement.WDB2.db2"))
                return new StorageList<WDB2.AchievementEntry>(fs);
        }

        [Benchmark(Description = "Achievement (WDC1)", OperationsPerInvoke = 5)]
        public StorageList<WDC1.AchievementEntry> WDC1()
        {
            using (var fs = OpenFile("Achievement.WDC1.db2"))
                return new StorageList<WDC1.AchievementEntry>(fs);
        }

        [Benchmark(Description = "Achievement (WDC2)", OperationsPerInvoke = 5)]
        public StorageList<WDC2.AchievementEntry> WDC2()
        {
            using (var fs = OpenFile("Achievement.WDC2.db2"))
                return new StorageList<WDC2.AchievementEntry>(fs);
        }
    }
}
