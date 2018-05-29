using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDB2
{
    [DBFileName(Name = "Item-sparse", Extension = FileExtension.DB2)]
    public sealed class ItemSparseEntry
    {
        [Index]
        public uint ID { get; set; }
        public uint Quality { get; set; }
        public uint Flags { get; set; }
        public uint Flags2 { get; set; }
        public float Unk430_1 { get; set; }
        public float Unk430_2 { get; set; }
        public uint BuyCount { get; set; }
        public uint BuyPrice { get; set; }
        public uint SellPrice { get; set; }
        public uint InventoryType { get; set; }
        public int AllowableClass { get; set; }
        public int AllowableRace { get; set; }
        public uint ItemLevel { get; set; }
        public int RequiredLevel { get; set; }
        public uint RequiredSkill { get; set; }
        public uint RequiredSkillRank { get; set; }
        public uint RequiredSpell { get; set; }
        public uint RequiredHonorRank { get; set; }
        public uint RequiredCityRank { get; set; }
        public uint RequiredReputationFaction { get; set; }
        public uint RequiredReputationRank { get; set; }
        public uint MaxCount { get; set; }
        public uint Stackable { get; set; }
        public uint ContainerSlots { get; set; }

        [Cardinality(SizeConst = 10)]
        public int[] ItemStatType { get; set; }

        [Cardinality(SizeConst = 10)]
        public uint[] ItemStatValue { get; set; }

        [Cardinality(SizeConst = 10)]
        public int[] ItemStatUnk1 { get; set; }

        [Cardinality(SizeConst = 10)]
        public int[] ItemStatUnk2 { get; set; }

        public uint ScalingStatDistribution { get; set; }
        public uint DamageType { get; set; }
        public uint Delay { get; set; }
        public float RangedModRange { get; set; }

        [Cardinality(SizeConst = 5)]
        public int[] SpellId { get; set; }

        [Cardinality(SizeConst = 5)]
        public int[] SpellTrigger { get; set; }

        [Cardinality(SizeConst = 5)]
        public int[] SpellCharges { get; set; }

        [Cardinality(SizeConst = 5)]
        public int[] SpellCooldown { get; set; }

        [Cardinality(SizeConst = 5)]
        public int[] SpellCategory { get; set; }

        [Cardinality(SizeConst = 5)]
        public int[] SpellCategoryCooldown { get; set; }

        public uint Bonding { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Name4 { get; set; }
        public string Description { get; set; }
        public uint PageText { get; set; }
        public uint LanguageID { get; set; }
        public uint PageMaterial { get; set; }
        public uint StartQuest { get; set; }
        public uint LockID { get; set; }
        public int Material { get; set; }
        public uint Sheath { get; set; }
        public uint RandomProperty { get; set; }
        public uint RandomSuffix { get; set; }
        public uint ItemSet { get; set; }
        public uint Area { get; set; }
        public uint Map { get; set; }
        public uint BagFamily { get; set; }
        public uint TotemCategory { get; set; }

        [Cardinality(SizeConst = 3)]
        public uint[] Color { get; set; }

        [Cardinality(SizeConst = 3)]
        public uint[] Content { get; set; }

        public int SocketBonus { get; set; }
        public uint GemProperties { get; set; }
        public float ArmorDamageModifier { get; set; }
        public uint Duration { get; set; }
        public uint ItemLimitCategory { get; set; }
        public uint HolidayId { get; set; }
        public float StatScalingFactor { get; set; }
        public int CurrencySubstitutionId { get; set; }
        public int CurrencySubstitutionCount { get; set; }
    }

    [DBFileName(Name = "Item-sparse", Extension = FileExtension.DB2)]
    public sealed class ItemSparseFieldEntry
    {
        [Index]
        public uint ID;
        public uint Quality;
        public uint Flags;
        public uint Flags2;
        public float Unk430_1;
        public float Unk430_2;
        public uint BuyCount;
        public uint BuyPrice;
        public uint SellPrice;
        public uint InventoryType;
        public int AllowableClass;
        public int AllowableRace;
        public uint ItemLevel;
        public int RequiredLevel;
        public uint RequiredSkill;
        public uint RequiredSkillRank;
        public uint RequiredSpell;
        public uint RequiredHonorRank;
        public uint RequiredCityRank;
        public uint RequiredReputationFaction;
        public uint RequiredReputationRank;
        public uint MaxCount;
        public uint Stackable;
        public uint ContainerSlots;

        [Cardinality(SizeConst = 10)]
        public int[] ItemStatType;

        [Cardinality(SizeConst = 10)]
        public uint[] ItemStatValue;

        [Cardinality(SizeConst = 10)]
        public int[] ItemStatUnk1;

        [Cardinality(SizeConst = 10)]
        public int[] ItemStatUnk2;

        public uint ScalingStatDistribution;
        public uint DamageType;
        public uint Delay;
        public float RangedModRange;

        [Cardinality(SizeConst = 5)]
        public int[] SpellId;

        [Cardinality(SizeConst = 5)]
        public int[] SpellTrigger;

        [Cardinality(SizeConst = 5)]
        public int[] SpellCharges;

        [Cardinality(SizeConst = 5)]
        public int[] SpellCooldown;

        [Cardinality(SizeConst = 5)]
        public int[] SpellCategory;

        [Cardinality(SizeConst = 5)]
        public int[] SpellCategoryCooldown;

        public uint Bonding;
        public string Name;
        public string Name2;
        public string Name3;
        public string Name4;
        public string Description;
        public uint PageText;
        public uint LanguageID;
        public uint PageMaterial;
        public uint StartQuest;
        public uint LockID;
        public int Material;
        public uint Sheath;
        public uint RandomProperty;
        public uint RandomSuffix;
        public uint ItemSet;
        public uint Area;
        public uint Map;
        public uint BagFamily;
        public uint TotemCategory;

        [Cardinality(SizeConst = 3)]
        public uint[] Color;

        [Cardinality(SizeConst = 3)]
        public uint[] Content;

        public int SocketBonus;
        public uint GemProperties;
        public float ArmorDamageModifier;
        public uint Duration;
        public uint ItemLimitCategory;
        public uint HolidayId;
        public float StatScalingFactor;
        public int CurrencySubstitutionId;
        public int CurrencySubstitutionCount;
    }
}