using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDB2
{
    [DBFileName(Name = "Achievement.WDB2", Extension = FileExtension.DB2)]
    public sealed class AchievementEntry
    {
        [Index]
        public int ID { get; set; }
        public int Faction { get; set; }
        public int MapID { get; set; }
        public uint Supercedes { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public uint Category { get; set; }
        public uint Points { get; set; }
        public uint UIOrder { get; set; }
        public uint Flags { get; set; }
        public uint IconID { get; set; }
        public string Rewards { get; set; }
        public uint MinimumCriteria { get; set; }
        public uint SharesCriteria { get; set; }
    }

	[DBFileName(Name = "Achievement.WDB2", Extension = FileExtension.DB2)]
	public sealed class AchievementEntry_725_24393
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public uint Flags { get; set; }
		public string Rewards { get; set; }
		public ushort MapID { get; set; }
		public ushort Supercedes { get; set; }
		public ushort Category { get; set; }
		public ushort UIOrder { get; set; }
		public ushort SharesCriteria { get; set; }
		public ushort CriteriaTree { get; set; }
		public byte Faction { get; set; }
		public byte Points { get; set; }
		public byte MinimumCriteria { get; set; }
		[Index]
		public ushort ID { get; set; }
		public uint IconID { get; set; } // this is a 24bit integer
	}
}
