using DBClientFiles.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Data.WDC1
{
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
