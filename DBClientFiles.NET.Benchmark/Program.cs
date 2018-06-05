using BenchmarkDotNet.Running;

namespace DBClientFiles.NET.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var switcher = new BenchmarkSwitcher(new[]
            {
                typeof(SimpleSpeedMeasurements),
                typeof(OptionsTest),
                typeof(SpanTest)
            });

            switcher.Run();
        }
    }
}
