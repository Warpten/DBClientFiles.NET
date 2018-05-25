using System;

namespace DBClientFiles.NET.Data
{
    public enum FileExtension
    {
        DBC,
        DB2
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DBFileNameAttribute : Attribute
    {
        public string Name { get; set; }
        public FileExtension Extension { get; set; } = FileExtension.DB2;
    }
}
