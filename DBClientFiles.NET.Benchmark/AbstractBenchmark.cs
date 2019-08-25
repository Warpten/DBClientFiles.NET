using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DBClientFiles.NET.Benchmark
{
    [MemoryDiagnoser]
    public abstract class AbstractBenchmark
    {
        protected MemoryStream File { get; private set; }

        protected static Consumer Consumer { get; } = new Consumer();

        private string Path { get; }

        protected AbstractBenchmark(string filePath)
        {
            Path = filePath;
        }

        [IterationSetup]
        public void IterationSetup()
        {
            File.Position = 0;
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            File = new MemoryStream();
            using (var fs = System.IO.File.OpenRead(Path))
                fs.CopyTo(File);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            File?.Dispose();
        }
    }
}
