using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDB2
{
    [DBFileName(Name = "Achievement_Category", Extension = FileExtension.DBC)]
    public sealed class AchievementCategoryEntry
    {
        [Index]
        public uint Id { get; set; }
        public int ParentCategory { get; set; }
        public string Name { get; set; }
        public uint SortOrder { get; set; }
    }
}