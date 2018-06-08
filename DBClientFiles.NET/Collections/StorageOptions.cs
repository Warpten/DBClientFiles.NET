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

    public class StorageOptions
    {
        public MemberTypes MemberType { get; set; }

        public LoadMask LoadMask { get; set; }

        public bool InternStrings { get; set; }

        /// <summary>
        /// If set, the library will ignore any file metadata information regarding the sign of each member.
        /// </summary>
        public bool OverrideSignedChecks { get; set; }

        /// <summary>
        /// If set to to <code>true</code>, the stream used as source will be copied to RAM before being used.
        /// This is set to true by default for anything but MemoryStream.
        /// </summary>
        public bool CopyToMemory { get; set; }

        public static StorageOptions Default { get; } = new StorageOptions
        {
            MemberType = MemberTypes.Property,
            LoadMask = LoadMask.Records,
            InternStrings = false,
            CopyToMemory = false,
            OverrideSignedChecks = false
        };
    }
}
