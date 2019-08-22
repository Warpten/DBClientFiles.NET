using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Benchmark.Attributes;
using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Types.WDBC;

namespace DBClientFiles.NET.Benchmark.WDBC
{
    [CoreJob, DisplayName("WDBC")]
    public class Benchmark
    {
        [Benchmark(Description = "Achievement.dbc (WDBC)")]
        public StorageList<Achievement> AchievementWDBC()
        {
            using (var fs = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\Achievement.dbc"))
                return new StorageList<Achievement>(StorageOptions.Default, fs);
        }
    }
}
