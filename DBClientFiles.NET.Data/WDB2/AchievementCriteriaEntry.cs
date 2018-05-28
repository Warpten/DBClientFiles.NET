using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDB2
{
    [DBFileName(Name = "Achievement_Criteria", Extension = FileExtension.DBC)]
    public sealed class AchievementCriteriaEntry
    {
        [Index]
        public uint Id { get; set; }
        public uint AchievementId { get; set; }
        public uint Type { get; set; }
        public uint MainRequirement { get; set; }
        public ulong MainRequirementCount { get; set; }
        public uint ExtraReqType_0 { get; set; }
        public uint ExtraReqValue_0 { get; set; }
        public uint ExtraReqType_1 { get; set; }
        public uint ExtraReqValue_1 { get; set; }
        public string Name { get; set; }
        public uint CompletionFlags { get; set; }
        public uint TimedCriteriaStartType { get; set; }
        public uint TimedCriteriaMiscId { get; set; }
        public uint TimerLimit { get; set; }
        public uint ShowOrder { get; set; }

        [Cardinality(SizeConst = 2)]
        public uint[] _Unk_1 { get; set; }

        public uint ExtraConditionType_1 { get; set; }
        public uint ExtraConditionType_2 { get; set; }
        public uint ExtraConditionType_3 { get; set; }
        public uint ExtraConditionValue_1 { get; set; }
        public uint ExtraConditionValue_2 { get; set; }
        public uint ExtraConditionValue_3 { get; set; }
    }
}