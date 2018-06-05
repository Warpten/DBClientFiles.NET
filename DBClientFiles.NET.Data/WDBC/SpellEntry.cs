using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDBC
{
    public sealed class SpellEntry
    {
        [Index] public int ID { get; set; }
        public uint Category { get; set; }
        public uint Dispel { get; set; }
        public uint Mechanic { get; set; }
        public uint Attributes { get; set; }
        public uint AttributesEx { get; set; }
        public uint AttributesEx2 { get; set; }
        public uint AttributesEx3 { get; set; }
        public uint AttributesEx4 { get; set; }
        public uint AttributesEx5 { get; set; }
        public uint AttributesEx6 { get; set; }
        public uint AttributesEx7 { get; set; }
        [Cardinality(SizeConst = 2)] public uint[] Stances { get; set; }
        [Cardinality(SizeConst = 2)] public uint[] StancesNot { get; set; }
        public uint Targets { get; set; }
        public uint TargetCreatureType { get; set; }
        public uint RequiresSpellFocus { get; set; }
        public uint FacingCasterFlags { get; set; }
        public uint CasterAuraState { get; set; }
        public uint TargetAuraState { get; set; }
        public uint CasterAuraStateNot { get; set; }
        public uint TargetAuraStateNot { get; set; }
        public uint casterAuraSpell { get; set; }
        public uint targetAuraSpell { get; set; }
        public uint excludeCasterAuraSpell { get; set; }
        public uint excludeTargetAuraSpell { get; set; }
        public uint CastingTimeIndex { get; set; }
        public uint RecoveryTime { get; set; }
        public uint CategoryRecoveryTime { get; set; }
        public uint InterruptFlags { get; set; }
        public uint AuraInterruptFlags { get; set; }
        public uint ChannelInterruptFlags { get; set; }
        public uint procFlags { get; set; }
        public uint procChance { get; set; }
        public uint procCharges { get; set; }
        public uint maxLevel { get; set; }
        public uint baseLevel { get; set; }
        public uint spellLevel { get; set; }
        public uint DurationIndex { get; set; }
        public uint powerType { get; set; }
        public uint manaCost { get; set; }
        public uint manaCostPerlevel { get; set; }
        public uint manaPerSecond { get; set; }
        public uint manaPerSecondPerLevel { get; set; }
        public uint rangeIndex { get; set; }
        public float speed { get; set; }
        public uint modalNextSpell { get; set; }
        public uint StackAmount { get; set; }
        [Cardinality(SizeConst = 2)] public uint[] Totem { get; set; }
        [Cardinality(SizeConst = 8)] public int[] Reagent { get; set; }
        [Cardinality(SizeConst = 8)] public uint[] ReagentCount { get; set; }
        public int EquippedItemClass { get; set; }
        public int EquippedItemSubClassMask { get; set; }
        public int EquippedItemInventoryTypeMask { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] Effect { get; set; }
        [Cardinality(SizeConst = 3)] public int[] EffectDieSides { get; set; }
        [Cardinality(SizeConst = 3)] public float[] EffectRealPointsPerLevel { get; set; }
        [Cardinality(SizeConst = 3)] public int[] EffectBasePoints { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] EffectMechanic { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] EffectImplicitTargetA { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] EffectImplicitTargetB { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] EffectRadiusIndex { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] EffectApplyAuraName { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] EffectAmplitude { get; set; }
        [Cardinality(SizeConst = 3)] public float[] EffectValueMultiplier { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] EffectChainTarget { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] EffectItemType { get; set; }
        [Cardinality(SizeConst = 3)] public int[] EffectMiscValue { get; set; }
        [Cardinality(SizeConst = 3)] public int[] EffectMiscValueB { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] EffectTriggerSpell { get; set; }
        [Cardinality(SizeConst = 3)] public float[] EffectPointsPerComboPoint { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] EffectClassMaskA { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] EffectClassMaskB { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] EffectClassMaskC { get; set; }
        [Cardinality(SizeConst = 2)] public uint[] SpellVisual { get; set; }
        public uint SpellIconID { get; set; }
        public uint activeIconID { get; set; }
        public uint SpellPriority { get; set; }
        [Cardinality(SizeConst = 16)] public string[] Name { get; set; }
        public int NameFlags { get; set; }
        [Cardinality(SizeConst = 16)] public string[] Rank { get; set; }
        public int RankFlags { get; set; }
        [Cardinality(SizeConst = 16)] public string[] Description { get; set; }
        public int DescriptionFlags { get; set; }
        [Cardinality(SizeConst = 16)] public string[] Tooltip { get; set; }
        public int TooltipFlags { get; set; }
        public uint ManaCostPercentage { get; set; }
        public uint StartRecoveryCategory { get; set; }
        public uint StartRecoveryTime { get; set; }
        public uint MaxTargetLevel { get; set; }
        public uint SpellFamilyName { get; set; }
        [Cardinality(SizeConst = 3)] public uint[] SpellFamilyFlags { get; set; }
        public uint MaxAffectedTargets { get; set; }
        public uint DmgClass { get; set; }
        public uint PreventionType { get; set; }
        public uint StanceBarOrder { get; set; }
        [Cardinality(SizeConst = 3)] public float[] EffectDamageMultiplier { get; set; }
        public uint MinFactionID { get; set; }
        public uint MinReputationID { get; set; }
        public uint RequiredAuraVision { get; set; }
        /// For some absurd reason i have 222 columns locally instead of 234
        // [Cardinality(SizeConst = 2)] public uint[] TotemCategory { get; set; }
        // public int AreaGroupId { get; set; }
        // public uint SchoolMask { get; set; }
        // public uint runeCostID { get; set; }
        // public uint spellMissileID { get; set; }
        // public uint PowerDisplayID { get; set; }
        // [Cardinality(SizeConst = 3)] public float[] EffectBonusMultiplier { get; set; }
        // public uint SpellDescriptionVariablesID { get; set; }
        // public uint SpellDifficultyID { get; set; }
    }
}
