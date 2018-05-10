using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Test.Structures.WDB2
{
    public sealed class ItemEntry
    {
        [Index]
        public int Id { get; set; }
        public uint Class { get; set; }
        public uint SubClass { get; set; }
        public int SoundOverrideSubclass { get; set; }
        public int Material { get; set; }
        public uint DisplayId { get; set; }
        public uint InventoryType { get; set; }
        public uint Sheath { get; set; }
    }
}
