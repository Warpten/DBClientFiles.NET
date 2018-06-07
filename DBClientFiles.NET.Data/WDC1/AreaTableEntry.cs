using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDC1
{
    [DBFileName(Name = "AreaTable", Extension = FileExtension.DB2)]
    public sealed class AreaTableEntry
    {
        [Index]
        public int ID { get; set; }
        public string ZoneName { get; set; }
        public string AreaName { get; set; }
        public int[] Flags { get; set; }
        public float AmbientMultiplier { get; set; }
        public ushort ContinentID { get; set; }
        public ushort ParentAreaID { get; set; }
        public short AreaBit { get; set; }
        public ushort AmbienceID { get; set; }
        public ushort ZoneMusic { get; set; }
        public ushort IntroSound { get; set; }
        public ushort[] LiquidTypeID { get; set; }
        public ushort UwZoneMusic { get; set; }
        public ushort UwAmbience { get; set; }
        public short PvpCombatWorldStateID { get; set; }
        public byte SoundProviderPref { get; set; }
        public byte SoundProviderPrefUnderwater { get; set; }
        public byte ExplorationLevel { get; set; }
        public byte FactionGroupMask { get; set; }
        public byte MountFlags { get; set; }
        public byte WildBattlePetLevelMin { get; set; }
        public byte WildBattlePetLevelMax { get; set; }
        public byte WindSettingsID { get; set; }
        public uint UwIntroSound { get; set; }
    }
}