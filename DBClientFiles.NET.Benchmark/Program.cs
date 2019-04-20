using BenchmarkDotNet.Running;

namespace DBClientFiles.NET.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summaries = BenchmarkSwitcher.FromTypes(new[] {
                typeof(Benchmarks.ComparativeBenchmark)

                // typeof(LanguageFeatureBenchmarks)
            }).Run(args);
        }
    }
}
