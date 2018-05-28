using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDB2
{
    public sealed class AreaPOIEntry
    {
        [Index]
        public int ID { get; set; }
        public int Important { get; set; }
        [Cardinality(SizeConst = 9)]
        public uint[] Icon { get; set; }
        public int FactionID { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public uint MapID { get; set; }
        public int Flags { get; set; }
        public int ZoneID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int WorldState { get; set; }
        public int WorldMapLink { get; set; }
        public int Unk2 { get; set; }
    }
}