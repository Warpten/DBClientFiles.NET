using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;

namespace DBClientFiles.NET.Benchmark.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class NetFramework472JobAttribute : Attribute, IConfigSource
    {
        public NetFramework472JobAttribute()
        {
            var job = Job.Default.With(Runtime.Clr).With(Jit.RyuJit).With(Platform.X64).With(CsProjClassicNetToolchain.From("net472"));
            Config = ManualConfig.CreateEmpty().With(job);
        }

        public IConfig Config { get; }
    }
}