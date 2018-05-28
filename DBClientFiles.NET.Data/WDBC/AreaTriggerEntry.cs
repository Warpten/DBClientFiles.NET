using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDBC
{
    [DBFileName(Name = "AreaTrigger", Extension = FileExtension.DBC)]
    public sealed class AreaTriggerEntry
    {
        [Index]
        public uint ID { get; set; }
        public uint MapID { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float radius { get; set; }
        public float box_x { get; set; }
        public float box_y { get; set; }
        public float box_z { get; set; }
        public float box_orientation { get; set; }
    }
}