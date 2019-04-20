using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Benchmark.DataTypes
{
    public sealed class LocString
    {
        [Cardinality(SizeConst = 16)]
        public string[] Values { get; set; }
        public uint Mask { get; set; }
    }
}
