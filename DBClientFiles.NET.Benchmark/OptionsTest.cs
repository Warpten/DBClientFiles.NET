using System.IO;
using BenchmarkDotNet.Attributes;
using WDBC = DBClientFiles.NET.Data.WDBC;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Collections.Generic;

namespace DBClientFiles.NET.Benchmark
{
    public class OptionsTest
    {
        private static string PATH_ROOT = @"C:\Users\Vincent Piquet\source\repos\DBClientFiles.NET\DBClientFiles.NET.Benchmark\bin\Release\net472\Data";

        [Benchmark(Description = "Spell.dbc")]
        public StorageList<WDBC.SpellEntry> Spell() => TestList<WDBC.SpellEntry>(_spell);
        [Benchmark(Description = "Achievement.dbc")]
        public StorageList<WDBC.AchievementEntry> Achievement() => TestList<WDBC.AchievementEntry>(_achievement);

        private StorageList<TStorage> TestList<TStorage>(Stream inputStream)
            where TStorage : class, new()
        {
            var options = new StorageOptions {
                CopyToMemory = _copyToMemory,
                InternStrings = _internStrings
            };

            inputStream.Position = 0;
            return new StorageList<TStorage>(inputStream, options);
        }

        [Params(true, false)]
        public bool _internStrings;

        [Params(true, false)]
        public bool _copyToMemory;

        [GlobalSetup]
        public void PrepareDataStreams()
        {
            _spell = File.OpenRead($@"{PATH_ROOT}\WDBC\Spell.dbc");
            _achievement = File.OpenRead($@"{PATH_ROOT}\WDBC\Achievement.dbc");
        }

        [GlobalCleanup]
        public void DisposeDataStreams()
        {
            _spell.Dispose();
            _achievement.Dispose();
        }

        private Stream _spell, _achievement;
    }
}
