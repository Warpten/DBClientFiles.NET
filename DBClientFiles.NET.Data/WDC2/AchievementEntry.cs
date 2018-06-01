using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDC2
{
    [DBFileName(Name = "Achievement", Extension = FileExtension.DB2)]
    public sealed class AchievementEntry
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Reward { get; set; }
        public int Flags { get; set; }
        public short InstanceID { get; set; }
        public short Supercedes { get; set; }
        public short Category { get; set; }
        public short UiOrder { get; set; }
        public short SharesCriteria { get; set; }
        public byte Faction { get; set; }
        public byte Points { get; set; }
        public byte MinimumCriteria { get; set; }
        [Index]
        public int ID { get; set; }
        public int IconFileID { get; set; }
        public uint CriteriaTree { get; set; }
    }
}
