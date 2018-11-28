using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.InProcess;

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
        }

        public IConfig Config { get; }
    }
}
