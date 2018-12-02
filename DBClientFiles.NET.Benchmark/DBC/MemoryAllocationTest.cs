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

        [Benchmark(Description = "AreaTrigger (single) (class)")]
        public uint SingleAreaTrigger()
        {
            _fileStream.Position = 0;

            var container = new StorageEnumerable<AreaTriggerEntry>(in StorageOptions.Default, _fileStream);
            return container.First().ID;
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fileStream = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\AreaTrigger.dbc");
        }
    }

    public class PreparationCost
    {
        private Stream _fileStream;
        private IEnumerable<AreaTriggerEntry> _enumerable;

        /*[Benchmark(Description = "Unique deserialization time", Baseline = true)]
        public AreaTriggerEntry UniqueCachedDeserialization()
        {
            return _enumerable.First();
        }*/

        [Benchmark(Description = "Full deserialization time")]
        public AreaTriggerEntry FullDeserialization()
        {
            _fileStream.Position = 0;

            return new StorageList<AreaTriggerEntry>(in StorageOptions.Default, _fileStream).First();
        }

        [Benchmark(Description = "Initial deserialization time")]
        public AreaTriggerEntry FirstDeserialization()
        {
            _fileStream.Position = 0;

            return new StorageEnumerable<AreaTriggerEntry>(in StorageOptions.Default, _fileStream).First();
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            using (var fileStream = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\AreaTrigger.dbc"))
                _enumerable = new StorageList<AreaTriggerEntry>(in StorageOptions.Default, fileStream);

            // This causes preparation to happen and thus every subsequent call to First() will
            // not initialize anything.
            DeadCodeEliminationHelper.KeepAliveWithoutBoxing(_enumerable.First());

            _fileStream = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\AreaTrigger.dbc");
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _fileStream.Dispose();
        }
    }
}
