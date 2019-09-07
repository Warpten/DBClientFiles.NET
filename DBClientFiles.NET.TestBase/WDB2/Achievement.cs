using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Types.WDB2
{
    public sealed class Achievement
    {
        [Index]
        public uint ID { get; set; }
        public int FactionID { get; set; }
        public int MapID { get; set; }
        public int ParentAchievementID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public uint CategoryID { get; set; }
        public uint Points { get; set; }
        public uint UIOrder { get; set; }
        public uint Flags { get; set; }
        public uint IconID { get; set; }
        public string Reward { get; set; }
        public uint MinimumCriteriaID { get; set; }
        public uint SharesCriteria { get; set; }
    }
}
