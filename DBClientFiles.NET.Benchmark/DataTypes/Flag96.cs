using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Benchmark.DataTypes
{
    public sealed class Flag96
    {
        [Cardinality(SizeConst = 3)]
        public uint[] Value { get; }
    }
}
