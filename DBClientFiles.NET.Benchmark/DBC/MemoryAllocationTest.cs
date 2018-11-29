using BenchmarkDotNet.Attributes;
using System.IO;
using DBClientFiles.NET.Collections.Generic;
using System.Linq;
using DBClientFiles.NET.Benchmark.Attributes;
using BenchmarkDotNet.Engines;

namespace DBClientFiles.NET.Benchmark.DBC
{
    [InProcess, MemoryDiagnoser]
    public class MemoryAllocationTest
    {
        private Stream _fileStream;

        [Benchmark(Description = "AreaTrigger (all) (class)")]
        public StorageList<AreaTriggerEntry> AreaTrigger()
        {
            _fileStream.Position = 0;

            return new StorageList<AreaTriggerEntry>(in StorageOptions.Default, _fileStream);
        }

        [Benchmark(Description = "AreaTrigger (single) (class)")]
        public uint SingleAreaTrigger()
        {
            _fileStream.Position = 0;

            var container = new StorageEnumerable<AreaTriggerEntry>(in StorageOptions.Default, _fileStream);
            return container.First().ID;
        }

        [Benchmark(Description = "AreaTrigger (single) allocation")]
        public void AreaTriggerAllocation()
        {
            DeadCodeEliminationHelper.KeepAliveWithoutBoxing(new AreaTriggerEntry());
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fileStream = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\AreaTrigger.dbc");
        }
    }
}
