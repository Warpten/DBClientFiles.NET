using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDC1
{
    public sealed class ItemSearchNameEntry
    {
        public long AllowableRace { get; set; }
        public string Display { get; set; }
        [Index]
        public int ID { get; set; }
        public int[] Flags { get; set; }
        public ushort ItemLevel { get; set; }
        public byte Quality { get; set; }
        public byte RequiredExpansion { get; set; }
        public byte RequiredLevel { get; set; }
        public ushort RequiredReputationFaction { get; set; }
        public byte RequiredReputationRank { get; set; }
        public int AllowableClass { get; set; }
        public ushort RequiredSkill { get; set; }
        public ushort RequiredSkillRank { get; set; }
        public uint RequiredSpell { get; set; }
    }
}