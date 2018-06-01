using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDBC
{
    [DBFileName(Name = "AreaTrigger", Extension = FileExtension.DBC)]
    public sealed class AreaTriggerEntry
    {
        [Index]
        public uint ID { get; set; }
        public uint MapID { get; set; }
        public C3Vector Position { get; set; }
        public float radius { get; set; }
        public float box_x { get; set; }
        public float box_y { get; set; }
        public float box_z { get; set; }
        public float box_orientation { get; set; }
    }

    public struct C3Vector
    {
        public C2Vector Vector2 { get; set; }
        public float Z { get; set; }
    }

    public struct C2Vector
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
}