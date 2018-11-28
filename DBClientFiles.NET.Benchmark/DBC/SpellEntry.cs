using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Benchmark.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBClientFiles.NET.Benchmark.DBC
{
    public sealed class SpellEntry
    {
        public uint Id;                                           // 0        m_ID
        public uint Category;                                     // 1        m_category
        public uint Dispel;                                       // 2        m_dispelType
        public uint Mechanic;                                     // 3        m_mechanic
        public uint Attributes;                                   // 4        m_attributes
        public uint AttributesEx;                                 // 5        m_attributesEx
        public uint AttributesEx2;                                // 6        m_attributesExB
        public uint AttributesEx3;                                // 7        m_attributesExC
        public uint AttributesEx4;                                // 8        m_attributesExD
        public uint AttributesEx5;                                // 9        m_attributesExE
        public uint AttributesEx6;                                // 10       m_attributesExF
        public uint AttributesEx7;                                // 11       m_attributesExG
        [Cardinality(SizeConst = 2)]
        public uint[] Stances;                                    // 12-13     m_shapeshiftMask
        [Cardinality(SizeConst = 2)]
        public uint[] StancesNot;                                 // 14-15    m_shapeshiftExclude
        public uint Targets;                                      // 16       m_targets
        public uint TargetCreatureType;                           // 17       m_targetCreatureType
        public uint RequiresSpellFocus;                           // 18       m_requiresSpellFocus
        public uint FacingCasterFlags;                            // 19       m_facingCasterFlags
        public uint CasterAuraState;                              // 20       m_casterAuraState
        public uint TargetAuraState;                              // 21       m_targetAuraState
        public uint CasterAuraStateNot;                           // 22       m_excludeCasterAuraState
        public uint TargetAuraStateNot;                           // 23       m_excludeTargetAuraState
        public uint casterAuraSpell;                              // 24       m_casterAuraSpell
        public uint targetAuraSpell;                              // 25       m_targetAuraSpell
        public uint excludeCasterAuraSpell;                       // 26       m_excludeCasterAuraSpell
        public uint excludeTargetAuraSpell;                       // 27       m_excludeTargetAuraSpell
        public uint CastingTimeIndex;                             // 28       m_castingTimeIndex
        public uint RecoveryTime;                                 // 29       m_recoveryTime
        public uint CategoryRecoveryTime;                         // 30       m_categoryRecoveryTime
        public uint InterruptFlags;                               // 31       m_interruptFlags
        public uint AuraInterruptFlags;                           // 32       m_auraInterruptFlags
        public uint ChannelInterruptFlags;                        // 33       m_channelInterruptFlags
        public uint procFlags;                                    // 34       m_procTypeMask
        public uint procChance;                                   // 35       m_procChance
        public uint procCharges;                                  // 36       m_procCharges
        public uint maxLevel;                                     // 37       m_maxLevel
        public uint baseLevel;                                    // 38       m_baseLevel
        public uint spellLevel;                                   // 39       m_spellLevel
        public uint DurationIndex;                                // 40       m_durationIndex
        public uint powerType;                                    // 41       m_powerType
        public uint manaCost;                                     // 42       m_manaCost
        public uint manaCostPerlevel;                             // 43       m_manaCostPerLevel
        public uint manaPerSecond;                                // 44       m_manaPerSecond
        public uint manaPerSecondPerLevel;                        // 45       m_manaPerSecondPerLeve
        public uint rangeIndex;                                   // 46       m_rangeIndex
        public float speed;                                       // 47       m_speed
        public uint modalNextSpell;                               // 48       m_modalNextSpell not used
        public uint StackAmount;                                  // 49       m_cumulativeAura
        [Cardinality(SizeConst = 2)]
        public uint[] Totem;                                      // 50-51    m_totem
        [Cardinality(SizeConst = 8)]
        public int[] Reagent;                                     // 52-59    m_reagent
        [Cardinality(SizeConst = 8)]
        public uint[] ReagentCount;                               // 60-67    m_reagentCount
        public int EquippedItemClass;                             // 68       m_equippedItemClass (value)
        public int EquippedItemSubClassMask;                      // 69       m_equippedItemSubclass (mask)
        public int EquippedItemInventoryTypeMask;                 // 70       m_equippedItemInvTypes (mask)
        [Cardinality(SizeConst = 3)]
        public uint[] Effect;                                     // 71-73    m_effect
        [Cardinality(SizeConst = 3)]
        public int[] EffectDieSides;                              // 74-76    m_effectDieSides
        [Cardinality(SizeConst = 3)]
        public float[] EffectRealPointsPerLevel;                  // 77-79    m_effectRealPointsPerLevel
        [Cardinality(SizeConst = 3)]
        public int[] EffectBasePoints;                            // 80-82    m_effectBasePoints (must not be used in spell/auras explicitly, must be used cached Spell::m_currentBasePoints)
        [Cardinality(SizeConst = 3)]
        public uint[] EffectMechanic;                             // 83-85    m_effectMechanic
        [Cardinality(SizeConst = 3)]
        public uint[] EffectImplicitTargetA;                      // 86-88    m_implicitTargetA
        [Cardinality(SizeConst = 3)]
        public uint[] EffectImplicitTargetB;                      // 89-91    m_implicitTargetB
        [Cardinality(SizeConst = 3)]
        public uint[] EffectRadiusIndex;                          // 92-94    m_effectRadiusIndex - spellradius.dbc
        [Cardinality(SizeConst = 3)]
        public uint[] EffectApplyAuraName;                        // 95-97    m_effectAura
        [Cardinality(SizeConst = 3)]
        public uint[] EffectAmplitude;                            // 98-100   m_effectAuraPeriod
        [Cardinality(SizeConst = 3)]
        public float[] EffectValueMultiplier;                     // 101-103
        [Cardinality(SizeConst = 3)]
        public uint[] EffectChainTarget;                          // 104-106  m_effectChainTargets
        [Cardinality(SizeConst = 3)]
        public uint[] EffectItemType;                             // 107-109  m_effectItemType
        [Cardinality(SizeConst = 3)]
        public int[] EffectMiscValue;                             // 110-112  m_effectMiscValue
        [Cardinality(SizeConst = 3)]
        public int[] EffectMiscValueB;                            // 113-115  m_effectMiscValueB
        [Cardinality(SizeConst = 3)]
        public uint[] EffectTriggerSpell;                         // 116-118  m_effectTriggerSpell
        [Cardinality(SizeConst = 3)]
        public float[] EffectPointsPerComboPoint;                 // 119-121  m_effectPointsPerCombo
        [Cardinality(SizeConst = 3)]
        public Flag96[] EffectSpellClassMask;                     // 122-130
        [Cardinality(SizeConst = 2)]
        public uint[] SpellVisual;                                // 131-132  m_spellVisualID
        public uint SpellIconID;                                  // 133      m_spellIconID
        public uint activeIconID;                                 // 134      m_activeIconID
        public uint SpellPriority;                                // 135      m_spellPriority
        [Cardinality(SizeConst = 16)]
        public string[] SpellName;                                // 136-151  m_name_lang
        public uint SpellNameFlag;                                // 152 not used
        [Cardinality(SizeConst = 16)]
        public string[] Rank;                                     // 153-168  m_nameSubtext_lang
        public uint RankFlags;                                    // 169 not used
        [Cardinality(SizeConst = 16)]
        public string[] Description;                              // 170-185  m_description_lang not used
        public uint DescriptionFlags;                             // 186 not used
        [Cardinality(SizeConst = 16)]
        public string[] ToolTip;                                  // 187-202  m_auraDescription_lang not used
        public uint ToolTipFlags;                                 // 203 not used
        public uint ManaCostPercentage;                           // 204      m_manaCostPct
        public uint StartRecoveryCategory;                        // 205      m_startRecoveryCategory
        public uint StartRecoveryTime;                            // 206      m_startRecoveryTime
        public uint MaxTargetLevel;                               // 207      m_maxTargetLevel
        public uint SpellFamilyName;                              // 208      m_spellClassSet
        [Cardinality(SizeConst = 3)]
        public Flag96[] SpellFamilyFlags;                         // 209-211
        public uint MaxAffectedTargets;                           // 212      m_maxTargets
        public uint DmgClass;                                     // 213      m_defenseType
        public uint PreventionType;                               // 214      m_preventionType
        public uint StanceBarOrder;                               // 215      m_stanceBarOrder not used
        [Cardinality(SizeConst = 3)]
        public float[] EffectDamageMultiplier;                    // 216-218  m_effectChainAmplitude
        public uint MinFactionId;                                 // 219      m_minFactionID not used
        public uint MinReputation;                                // 220      m_minReputation not used
        public uint RequiredAuraVision;                           // 221      m_requiredAuraVision not used
        [Cardinality(SizeConst = 2)]
        public uint[] TotemCategory;                              // 222-223  m_requiredTotemCategoryID
        public int AreaGroupId;                                   // 224      m_requiredAreaGroupId
        public uint SchoolMask;                                   // 225      m_schoolMask
        public uint runeCostID;                                   // 226      m_runeCostID
        public uint spellMissileID;                               // 227      m_spellMissileID not used
        public uint PowerDisplayId;                               // 228      PowerDisplay.dbc, new in 3.1
        [Cardinality(SizeConst = 3)]
        public float[] EffectBonusMultiplier;                     // 229-231  3.2.0
        public uint spellDescriptionVariableID;                   // 232      3.2.0
        public uint SpellDifficultyId;                            // 233      3.3.0
    };
}
