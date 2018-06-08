using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDC2
{
    [DBFileName(Name = "Achievement.WDC2", Extension = FileExtension.DB2)]
    public sealed class AchievementEntry
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Reward { get; set; }
        [Index]
        public int ID { get; set; }
        public int Flags { get; set; }
        public short InstanceID { get; set; }
        public short Supercedes { get; set; }
        public short Category { get; set; }
        public short UiOrder { get; set; }
        public short SharesCriteria { get; set; }
        public sbyte Faction { get; set; }
        public sbyte Points { get; set; }
        public sbyte MinimumCriteria { get; set; }
        public uint IconFileID { get; set; }
        public int CriteriaTree { get; set; }
    }
}
