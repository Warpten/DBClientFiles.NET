using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace DBClientFiles.NET.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summaries = BenchmarkRunner.Run(typeof(Benchmark), new DebugInProcessConfig());
        }
    }
}
