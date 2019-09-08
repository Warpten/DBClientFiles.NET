using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System.Linq;
using System.Reflection;

namespace DBClientFiles.NET.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summaries = BenchmarkSwitcher.FromTypes(
                Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(AbstractBenchmark).IsAssignableFrom(t) && !t.IsAbstract).ToArray()
                // new [] { typeof(WDBC.AchievementAllocationBenchmark) }
#if DEBUG
            ).Run(args, new DebugInProcessConfig());
#else
            ).Run(args);
#endif
        }
    }
}
