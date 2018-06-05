using System.IO;
using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Collections.Generic;

namespace DBClientFiles.NET.Benchmark
{
    /// <summary>
    /// A simple tester for speed.
    /// </summary>
    public class SimpleSpeedMeasurements
    {
        private static string PATH_ROOT = @"C:\Users\Vincent Piquet\source\repos\DBClientFiles.NET\DBClientFiles.NET.Benchmark\bin\Release\net472\Data";

        [Benchmark(Description = "Achievement.dbc (WDBC)")]
        public StorageList<Data.WDBC.AchievementEntry> AchievementWDBC() => new StorageList<Data.WDBC.AchievementEntry>(_achievement);

        [Benchmark(Description = "Item-sparse.db2 (WDB2)", OperationsPerInvoke = 1)]
        public StorageList<Data.WDB2.ItemSparseEntry> ItemSparseWDB2() => new StorageList<Data.WDB2.ItemSparseEntry>(_itemSparse);

        [Benchmark(Description = "SpellEffect.db2 (WDC1)", OperationsPerInvoke = 1)]
        public StorageList<Data.WDC1.SpellEffectEntry> SpellEffectWDC1() => new StorageList<Data.WDC1.SpellEffectEntry>(_spellEffect);

        [GlobalSetup]
        public void PrepareDataStreams()
        {
            _achievement = File.OpenRead($@"{PATH_ROOT}\WDBC\Achievement.dbc");
            _spellEffect = File.OpenRead($@"{PATH_ROOT}\WDC1\SpellEffect.db2");
            _itemSparse = File.OpenRead($@"{PATH_ROOT}\WDB2\Item-sparse.db2");
        }

        [GlobalCleanup]
        public void DisposeDataStreams()
        {
            _achievement.Dispose();
            _spellEffect.Dispose();
            _itemSparse.Dispose();
        }

        private Stream _achievement;
        private Stream _itemSparse;
        private Stream _spellEffect;
    }
}