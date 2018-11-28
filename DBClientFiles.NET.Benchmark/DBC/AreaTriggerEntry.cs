using DBClientFiles.NET.Benchmark.DataTypes;

namespace DBClientFiles.NET.Benchmark.DBC
{
    public sealed class AreaTriggerEntry
    {
        public uint ID { get; set; }
        public uint MapID { get; set; }
        public C3Vector Position { get; set; }
        public float Radius { get; set; }
        public C2Vector BoxSize { get; set; }
        public float BoxOrientation { get; set; }
    }
}
