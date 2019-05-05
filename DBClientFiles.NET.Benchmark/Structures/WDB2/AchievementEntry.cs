using System;
using System.Collections.Generic;
using System.Text;

namespace DBClientFiles.NET.Benchmark.Structures.WDB2
{
    public sealed class AchievementEntry
    {
        public uint Id { get; set; }
        public int RequiredFaction { get; set; }
        public int MapId { get; set; }
        public int ParentAchievement { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public uint Category { get; set; }
        public uint Points { get; set; }
        public uint OrderInCategory { get; set; }
        public uint Flags { get; set; }
        public uint Icon { get; set; }
        public string Reward { get; set; }
        public uint Count { get; set; }
        public uint ReferenceAchievement { get; set; }
    }
}
