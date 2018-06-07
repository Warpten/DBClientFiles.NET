using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDC1
{
    // [DBFileName(Name = "SpellEffect", Extension = FileExtension.DB2)]
    public sealed class SpellEffectEntry
    {
        [Index]
        public int ID { get; set; }
        public uint Effect { get; set; }
        public int EffectBasePoints { get; set; }
        public int EffectIndex { get; set; }
        public int EffectAura { get; set; }
        public int DifficultyID { get; set; }
        public float EffectAmplitude { get; set; }
        public int EffectAuraPeriod { get; set; }
        public float EffectBonusCoefficient { get; set; }
        public float EffectChainAmplitude { get; set; }
        public int EffectChainTargets { get; set; }
        public int EffectDieSides { get; set; }
        public int EffectItemType { get; set; }
        public int EffectMechanic { get; set; }
        public float EffectPointsPerResource { get; set; }
        public float EffectRealPointsPerLevel { get; set; }
        public int EffectTriggerSpell { get; set; }
        public float EffectPosFacing { get; set; }
        public int EffectAttributes { get; set; }
        public float BonusCoefficientFromAP { get; set; }
        public float PvpMultiplier { get; set; }
        public float Coefficient { get; set; }
        public float Variance { get; set; }
        public float ResourceCoefficient { get; set; }
        public float GroupSizeBasePointsCoefficient { get; set; }
        public int[] EffectSpellClassMask { get; set; }
        public int[] EffectMiscValue { get; set; }
        public uint[] EffectRadiusIndex { get; set; }
        public uint[] ImplicitTarget { get; set; }
        public int SpellID { get; set; }
    }
}