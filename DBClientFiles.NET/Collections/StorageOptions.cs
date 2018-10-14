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
        public bool IgnoreSignedChecks { get; set; }

        /// <summary>
        /// If set to to <c>true</c>, the stream used as source will be copied to RAM before being used.
        /// This is set to <c>true</c> by default for anything but MemoryStream.
        /// </summary>
        public bool CopyToMemory { get; set; }

        private static StorageOptions _default = new StorageOptions
        {
            MemberType = MemberTypes.Property,
            LoadMask = LoadMask.Records,
            InternStrings = false,
            CopyToMemory = false,
            IgnoreSignedChecks = false
        };

        public static ref readonly StorageOptions Default => ref _default;
    }
}
