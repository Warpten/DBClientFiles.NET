using DBClientFiles.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Data.WDB2
{
    [DBFileName(Name = "ItemSparse.WDB2", Extension = FileExtension.DB2)]
    public sealed class ItemSparseEntry_725_24393
    {
        [Index]
        public int ID { get; set; }
        [Cardinality(SizeConst = 3)]
        public uint Flags { get; set; }
        public float Field02 { get; set; }
        public float Field03 { get; set; }
        public uint BuyCount { get; set; }
        public uint BuyPrice { get; set; }
        public uint SellPrice { get; set; }
        public int AllowableRace { get; set; }
        public uint RequiredSpell { get; set; }
        public uint MaxCount { get; set; }
        public uint Stackable { get; set; }
        [Cardinality(SizeConst = 10)]
        public int ItemStatAllocation { get; set; }
        [Cardinality(SizeConst = 10)]
        public float ItemStatSocketCostMultiplier { get; set; }
        public float RangedModRange { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Name4 { get; set; }
        public string Description { get; set; }
        public uint BagFamily { get; set; }
        public float ArmorDamageModifier { get; set; }
        public uint Duration { get; set; }
        public float StatScalingFactor { get; set; }
        public ushort AllowableClass { get; set; }
        public ushort ItemLevel { get; set; }
        public ushort RequiredSkill { get; set; }
        public ushort RequiredSkillRank { get; set; }
        public ushort RequiredReputationFaction { get; set; }
        [Cardinality(SizeConst = 10)]
        public ushort ItemStatValue { get; set; }
        public ushort ScalingStatDistribution { get; set; }
        public ushort Delay { get; set; }
        public ushort PageText { get; set; }
        public ushort StartQuest { get; set; }
        public ushort LockID { get; set; }
        public ushort RandomProperty { get; set; }
        public ushort RandomSuffix { get; set; }
        public ushort ItemSet { get; set; }
        public ushort Area { get; set; }
        public ushort Map { get; set; }
        public ushort TotemCategory { get; set; }
        public ushort SocketBonus { get; set; }
        public ushort GemProperties { get; set; }
        public ushort ItemLimitCategory { get; set; }
        public ushort HolidayID { get; set; }
        public ushort RequiredTransmogHolidayID { get; set; }
        public ushort ItemNameDescriptionID { get; set; }
        public byte Quality { get; set; }
        public byte InventoryType { get; set; }
        public byte RequiredLevel { get; set; }
        public byte RequiredHonorRank { get; set; }
        public byte RequiredCityRank { get; set; }
        public byte RequiredReputationRank { get; set; }
        public byte ContainerSlots { get; set; }
        [Cardinality(SizeConst = 10)]
        public byte ItemStatType { get; set; }
        public byte DamageType { get; set; }
        public byte Bonding { get; set; }
        public byte LanguageID { get; set; }
        public byte PageMaterial { get; set; }
        public byte Material { get; set; }
        public byte Sheath { get; set; }
        [Cardinality(SizeConst = 3)]
        public byte SocketColor { get; set; }
        public byte CurrencySubstitutionID { get; set; }
        public byte CurrencySubstitutionCount { get; set; }
        public byte ArtifactID { get; set; }
        public byte RequiredExpansion { get; set; }

    }
}
