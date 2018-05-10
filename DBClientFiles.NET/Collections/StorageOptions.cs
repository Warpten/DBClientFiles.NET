using System;
using System.Reflection;

namespace DBClientFiles.NET.Collections
{
    [Flags]
    public enum LoadMask
    {
        Records,
        StringTable
    }

    public sealed class StorageOptions
    {
        public MemberTypes MemberType { get; set; } = MemberTypes.Property;
        public LoadMask LoadMask { get; set; } = LoadMask.Records;

        public bool InternStrings { get; set; } = true;
        public bool KeepStringTable { get; set; } = false;

        public static StorageOptions Default { get; } = new StorageOptions();
    }
}
