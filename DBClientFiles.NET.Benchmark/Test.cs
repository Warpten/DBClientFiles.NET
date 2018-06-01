using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;
using DBClientFiles.NET.Collections.Generic;
using WDBC = DBClientFiles.NET.Data.WDBC;
using WDB2 = DBClientFiles.NET.Data.WDB2;

namespace DBClientFiles.NET.Benchmark
{
    [MarkdownExporter, RPlotExporter, AsciiDocExporter]
    [ClrJob, CoreJob]
    [MinColumn, MaxColumn]
    public class Test
    {
        public static void Main() => BenchmarkRunner.Run<Test>();

        private static string PATH_ROOT = @"C:\Users\Vincent Piquet\source\repos\DBClientFiles.NET\DBClientFiles.NET.Benchmark\bin\Release\net472\Data";

        [Benchmark(OperationsPerInvoke = 200, Description = "Achievement.dbc (WDBC)")]
        public StorageList<WDBC.AchievementEntry> AchievementWDBC() => StructureTester<WDBC.AchievementEntry>($@"{PATH_ROOT}\WDBC\Achievement.dbc");
    
        [Benchmark(OperationsPerInvoke = 200, Description = "Item-sparse.db2 (WDB2)")]
        public StorageList<WDB2.ItemSparseEntry> ItemSparseWDB2() => StructureTester<WDB2.ItemSparseEntry>($@"{PATH_ROOT}\WDB2\Item-sparse.db2");
    
        private StorageList<T> StructureTester<T>(string fileName)
            where T : class, new()
        {
            using (var fileStream = File.OpenRead(fileName))
                return new StorageList<T>(fileStream);
        }
    }
}
