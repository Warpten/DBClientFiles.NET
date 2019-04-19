using BenchmarkDotNet.Attributes;
using System.IO;
using DBClientFiles.NET.Collections.Generic;
using System.Linq;
using DBClientFiles.NET.Benchmark.Attributes;
using BenchmarkDotNet.Engines;
using System.Collections.Generic;

namespace DBClientFiles.NET.Benchmark.DBC
{
    [MemoryDiagnoser, NetCoreJob, BenchmarkCategory("DBC")]
    public class DBC
    {
        private Stream _fileStream;

        [Benchmark(Description = "AreaTrigger (all) (class)")]
        public StorageList<AreaTriggerEntry> AreaTrigger()
        {
            _fileStream.Position = 0;

            return new StorageList<AreaTriggerEntry>(in StorageOptions.Default, _fileStream);
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fileStream = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\AreaTrigger.dbc");
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _fileStream.Dispose();
        }
    }
}
