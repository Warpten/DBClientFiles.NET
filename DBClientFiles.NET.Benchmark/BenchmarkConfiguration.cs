using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Exporters.Csv;

namespace DBClientFiles.NET.Benchmark
{
    public class BenchmarkConfiguration : ManualConfig
    {
        public BenchmarkConfiguration()
        {
            Add(Job.Default.With(Runtime.Core).With(Jit.RyuJit).With(Platform.X64).With(CsProjCoreToolchain.NetCoreApp21));
            Add(Job.Default.With(Runtime.Clr).With(Jit.RyuJit).With(Platform.X64).With(CsProjClassicNetToolchain.From("net472")));

            Add(new MinimalColumnProvider());
            Set(new DefaultOrderProvider(SummaryOrderPolicy.SlowestToFastest));

            Add(MarkdownExporter.GitHub);
            Add(RPlotExporter.Default);
            Add(CsvMeasurementsExporter.Default);
            Add(AsciiDocExporter.Default);

            Add(new ConsoleLogger());
        }

        private sealed class MinimalColumnProvider : IColumnProvider
        {
            public IEnumerable<IColumn> GetColumns(Summary summary)
            {
                yield return TargetMethodColumn.Method;
                yield return new JobCharacteristicColumn(InfrastructureMode.ToolchainCharacteristic);
                yield return StatisticColumn.Min;
                yield return StatisticColumn.Max;
                yield return StatisticColumn.Median;
                yield return StatisticColumn.Mean;
                yield return StatisticColumn.StdDev;
                yield return StatisticColumn.StdErr;
            }
        }
    }
}
