using DBClientFiles.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Data.WDB2
{
    [DBFileName(Name = "ItemSparse.WDB2", Extension = FileExtension.DB2)]
    public sealed class ItemSparseEntry_715_23420
    {
        [Index]
        public int ID { get; set; }
        [Cardinality(SizeConst = 3)]
        public int[] Field00 { get; set; }
        public float Field0C { get; set; }
        public float Field10 { get; set; }
        public int Field14 { get; set; }
        public int Field18 { get; set; }
        public int Field1C { get; set; }
        public int Field20 { get; set; }
        public int Field24 { get; set; }
        public int Field28 { get; set; }
        public int Field2C { get; set; }
        [Cardinality(SizeConst = 10)]
        public int[] Field30 { get; set; }
        [Cardinality(SizeConst = 10)]
        public float[] Field58 { get; set; }
        public float Field80 { get; set; }
        public string Field84 { get; set; }
        public string Field88 { get; set; }
        public string Field8C { get; set; }
        public string Field90 { get; set; }
        public string Field94 { get; set; }
        public int Field98 { get; set; }
        public float Field9C { get; set; }
        public int FieldA0 { get; set; }
        public float FieldA4 { get; set; }
        public ushort FieldA8 { get; set; }
        public ushort FieldAA { get; set; }
        public ushort FieldAC { get; set; }
        public ushort FieldAE { get; set; }
        [Cardinality(SizeConst = 10)]
        public ushort[] FieldB0 { get; set; }
        public ushort FieldC4 { get; set; }
        public ushort FieldC6 { get; set; }
        public ushort FieldC8 { get; set; }
        public ushort FieldCA { get; set; }
        public ushort FieldCC { get; set; }
        public ushort FieldCE { get; set; }
        public ushort FieldD0 { get; set; }
        public ushort FieldD2 { get; set; }
        public ushort FieldD4 { get; set; }
        public ushort FieldD6 { get; set; }
        public ushort FieldD8 { get; set; }
        public ushort FieldDA { get; set; }
        public ushort FieldDC { get; set; }
        public ushort FieldDE { get; set; }
        public ushort FieldE0 { get; set; }
        public byte FieldE2 { get; set; }
        public byte FieldE3 { get; set; }
        public byte FieldE4 { get; set; }
        public byte FieldE5 { get; set; }
        public byte FieldE6 { get; set; }
        public byte FieldE7 { get; set; }
        public byte FieldE8 { get; set; }
        public byte FieldE9 { get; set; }
        [Cardinality(SizeConst = 10)]
        public byte[] FieldEA { get; set; }
        public byte FieldF4 { get; set; }
        public byte FieldF5 { get; set; }
        public byte FieldF6 { get; set; }
        public byte FieldF7 { get; set; }
        public byte FieldF8 { get; set; }
        public byte FieldF9 { get; set; }
        public byte FieldFA { get; set; }
        [Cardinality(SizeConst = 3)]
        public byte[] FieldFB { get; set; }
        public byte FieldFE { get; set; }
        public byte FieldFF { get; set; }
        public byte Field100 { get; set; }
        public byte Field101 { get; set; }
    }


    [DBFileName(Name = "ItemSparse.WDB2", Extension = FileExtension.DB2)]
    public sealed class ItemSparseEntry_725_24393
    {
        [Index]
        public int ID { get; set; }
        [Cardinality(SizeConst = 3)]
        public uint[] Flags { get; set; }
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
        public int[] ItemStatAllocation { get; set; }
        [Cardinality(SizeConst = 10)]
        public float[] ItemStatSocketCostMultiplier { get; set; }
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
        public ushort[] ItemStatValue { get; set; }
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
        public byte[] ItemStatType { get; set; }
        public byte DamageType { get; set; }
        public byte Bonding { get; set; }
        public byte LanguageID { get; set; }
        public byte PageMaterial { get; set; }
        public byte Material { get; set; }
        public byte Sheath { get; set; }
        [Cardinality(SizeConst = 3)]
        public byte[] SocketColor { get; set; }
        public byte CurrencySubstitutionID { get; set; }
        public byte CurrencySubstitutionCount { get; set; }
        public byte ArtifactID { get; set; }
        public byte RequiredExpansion { get; set; }

    }
}
