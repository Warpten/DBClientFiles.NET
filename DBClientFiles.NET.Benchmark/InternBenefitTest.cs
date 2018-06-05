using System.IO;
using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Collections.Generic;
using WDBC = DBClientFiles.NET.Data.WDBC;

namespace DBClientFiles.NET.Benchmark
{
    public class InternBenefitTest
    {
        private static string PATH_ROOT = @"C:\Users\Vincent Piquet\source\repos\DBClientFiles.NET\DBClientFiles.NET.Benchmark\bin\Release\net472\Data";

        [Benchmark(Description = "Intern")]
        public StorageList<WDBC.SpellEntry> MemoryTest() => TestList<WDBC.SpellEntry>(_spell, _internOptions);

        [Benchmark(Description = "Don't intern", Baseline = true)]
        public StorageList<WDBC.SpellEntry> DiskTest() => TestList<WDBC.SpellEntry>(_spell, _noInterningOptions);

        private static StorageList<TStorage> TestList<TStorage>(Stream inputStream, StorageOptions options)
            where TStorage : class, new()
        {
            inputStream.Position = 0;
            return new StorageList<TStorage>(inputStream, options);
        }

        [GlobalSetup]
        public void PrepareDataStreams()
        {
            _internOptions = new StorageOptions {
                CopyToMemory = true,
                InternStrings = true
            };

            _noInterningOptions = new StorageOptions {
                CopyToMemory = true,
                InternStrings = false
            };

            _spell = File.OpenRead($@"{PATH_ROOT}\WDBC\Spell.dbc");
        }

        [GlobalCleanup]
        public void DisposeDataStreams()
        {
            _spell.Dispose();
        }

        private StorageOptions _internOptions, _noInterningOptions;
        private Stream _spell;
    }
}
