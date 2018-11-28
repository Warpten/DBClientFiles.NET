using System;
using System.Reflection;

namespace DBClientFiles.NET
{
    public readonly struct StorageOptions
    {
        public readonly MemberTypes MemberType;

        public readonly bool InternStrings;

        /// <summary>
        /// If set, the library will ignore any file metadata information regarding the sign of each member.
        /// </summary>
        public readonly bool IgnoreSignedChecks;

        /// <summary>
        /// If set to to <c>true</c>, the stream used as source will be copied to RAM before being used.
        /// This is set to <c>true</c> by default for anything but MemoryStream.
        /// </summary>
        public readonly bool CopyToMemory;

        public readonly bool ReadOnly;

        private static StorageOptions _default = new StorageOptions(
            memberType: MemberTypes.Property,
            internStrings: false,
            copyToMemory: false,
            ignoreSignedChecks: false,
            readOnly: true
        );

        public static ref readonly StorageOptions Default => ref _default;

        public StorageOptions(MemberTypes memberType,
            bool internStrings,
            bool ignoreSignedChecks,
            bool copyToMemory,
            bool readOnly)
        {
            MemberType = memberType;
            InternStrings = internStrings;
            IgnoreSignedChecks = ignoreSignedChecks;
            CopyToMemory = copyToMemory;
            ReadOnly = readOnly;
        }
    }
}
