﻿namespace DBClientFiles.NET.Types.WDC1
{
    public sealed class Achievement
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Reward { get; set; }
        public int Flags { get; set; }
        public int InstanceID { get; set; }
        public int Supercedes { get; set; }
        public int CategoryID { get; set; }
        public int UIOrder { get; set; }
        public int SharesCriteria { get; set; }
        public int Faction { get; set; }
        public int Points { get; set; }
        public int MinimumCriteria { get; set; }
        public int ID { get; set; }
        public int IconFileID { get; set; }
        public int CriteriaTreeID { get; set; }
    }
}
