﻿using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Types.Shared;

namespace DBClientFiles.NET.Types.WDBC
{
    public sealed class Achievement
    {
        [Index]
        public uint ID { get; set; }
        public int FactionID { get; set; }
        public int MapID { get; set; }
        public int ParentAchievementID { get; set; }
        public LocString Name { get; set; }
        public LocString Description { get; set; }
        public uint CategoryID { get; set; }
        public uint Points { get; set; }
        public uint UIOrder { get; set; }
        public uint Flags { get; set; }
        public uint IconID { get; set; }
        public LocString Rewards { get; set; }
        public uint MinimumCriteriaID { get; set; }
        public uint SharesCriteria { get; set; }
    }
}
