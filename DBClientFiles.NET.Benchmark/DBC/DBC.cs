using BenchmarkDotNet.Attributes;
using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Benchmark.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using System.Linq;
using System.Collections.Generic;

namespace DBClientFiles.NET.Benchmark.DBC
{
    [MinColumn, MaxColumn, InProcess]
    [MemoryDiagnoser]
    public class DBC
    {
        // [Benchmark(Description = "AreaTrigger (class)")]
        /*public StorageList<AreaTriggerEntry> AreaTrigger()
        {
            using (var fs = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\AreaTrigger.dbc"))
                return new StorageList<AreaTriggerEntry>(in StorageOptions.Default, fs);
        }*/

        [Benchmark(Description = "AreaTrigger (single) (class)")]
        public uint AreaTrigger()
        {
            using (var fs = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\AreaTrigger.dbc"))
            {
                var container = new StorageEnumerable<AreaTriggerEntry>(in StorageOptions.Default, fs);
                return container.First().ID;
            }
        }

        /*
        [Benchmark(Description = "Spell (class)")]
        public StorageList<SpellEntry> Spell()
        {
            using (var fs = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\Spell.dbc"))
                return new StorageList<SpellEntry>(StorageOptions.Default, fs);
        }

        [Benchmark(Description = "Achievement (class)")]
        public StorageList<AchievementEntry> Achievement()
        {
            using (var fs = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\Achievement.dbc"))
                return new StorageList<AchievementEntry>(StorageOptions.Default, fs);
        }*/
    }
}
