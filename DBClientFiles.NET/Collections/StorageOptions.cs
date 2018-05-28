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

        /// <summary>
        /// If set to to <code>true</code>, the stream used as source will be copied to RAM before being used.
        /// This is set to true by default for anything but MemoryStream.
        /// </summary>
        public bool CopyToMemory { get; set; } = false;

        public static StorageOptions Default { get; } = new StorageOptions();
    }
}
