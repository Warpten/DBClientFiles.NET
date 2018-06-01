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

namespace DBClientFiles.NET.Benchmark
{
    public class BenchmarkConfiguration : ManualConfig
    {
        public BenchmarkConfiguration()
        {
            Add(Job.Default.With(Platform.X64).With(CsProjClassicNetToolchain.Net471));
            Add(Job.Default.With(Platform.X64).With(CsProjCoreToolchain.NetCoreApp21));

            Add(MemoryDiagnoser.Default);
            Add(new MinimalColumnProvider());
            Add(MemoryDiagnoser.Default.GetColumnProvider());
            Set(new DefaultOrderProvider(SummaryOrderPolicy.SlowestToFastest));
            Add(MarkdownExporter.GitHub);
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
