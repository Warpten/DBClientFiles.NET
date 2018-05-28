using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDB2
{
    public sealed class AreaTableEntry
    {
        [Index]
        public uint Id { get; set; }
        public uint MapId { get; set; }
        public uint ZoneId { get; set; }
        public uint ExploreFlag { get; set; }
        public uint Flags { get; set; }
        public uint SoundPreferences { get; set; }
        public uint SoundPreferencesUnderwater { get; set; }
        public uint SoundAmbience { get; set; }
        public uint ZoneMusic { get; set; }
        public uint ZoneIntroMusicTable { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public uint FactionGroupMask { get; set; }
        public uint LiquidType1 { get; set; }
        public uint LiquidType2 { get; set; }
        public uint LiquidType3 { get; set; }
        public uint LiquidType4 { get; set; }
        public float MaxDepth { get; set; }
        public float AmbientMultiplier { get; set; }
        public uint Light { get; set; }
        public uint UnkCataclysm1 { get; set; }
        public uint UnkCataclysm2 { get; set; }
        public uint UnkCataclysm3 { get; set; }
        public uint UnkCataclysm4 { get; set; }
        public int UnkCataclysm5 { get; set; }
        public int UnkCataclysm6 { get; set; }
    }
}