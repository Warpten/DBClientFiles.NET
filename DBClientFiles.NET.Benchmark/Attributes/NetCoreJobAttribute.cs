using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;

namespace DBClientFiles.NET.Benchmark.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class NetCoreJobAttribute : Attribute, IConfigSource
    {
        public NetCoreJobAttribute()
        {
            var job = Job.Default
                .With(Runtime.Core)
                .With(Jit.RyuJit)
                .With(Platform.X64)
                .With(CsProjCoreToolchain.NetCoreApp21);
            Config = ManualConfig.CreateEmpty().With(job);
            Config = Config.With(ConfigOptions.DisableOptimizationsValidator);
        }

        public IConfig Config { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class Net461Attribute : Attribute, IConfigSource
    {
        public Net461Attribute()
        {
            var job = Job.Default
                .With(Runtime.Clr)
                .With(Jit.RyuJit)
                .With(Platform.X64)
                .With(CsProjClassicNetToolchain.Net461);
            Config = ManualConfig.CreateEmpty().With(job);
            Config = Config.With(ConfigOptions.DisableOptimizationsValidator);
        }

        public IConfig Config { get; }
    }
}
