﻿using System.Reflection;
using System.Text;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils.Extensions;

namespace DBClientFiles.NET
{
    public readonly struct StorageOptions
    {
        public readonly MemberTypes MemberType;

        internal TypeTokenType TokenType => MemberType.ToTypeToken();

        public readonly bool InternStrings;

        /// <summary>
        /// If set, the library will ignore any file metadata information regarding the sign of each member.
        /// </summary>
        public readonly bool IgnoreSignedChecks;

        /// <summary>
        /// The encoding to use when reading text from the file. This defaults to UTF-8.
        /// </summary>
        public readonly Encoding Encoding;

        /// <summary>
        /// If set to to <c>true</c>, the stream used as source will be copied to RAM before being used.
        /// This is set to <c>true</c> by default for anything but MemoryStream.
        /// </summary>
        public readonly bool CopyToMemory;

        /// <summary>
        /// Defines wether or not the collection being created should allow being mutated.
        /// </summary>
        public readonly bool ReadOnly;

        private static StorageOptions _default = new StorageOptions(
            memberType: MemberTypes.Property,
            internStrings: false,
            copyToMemory: false,
            ignoreSignedChecks: false,
            readOnly: true,
            encoding: Encoding.UTF8
        );

        public static ref readonly StorageOptions Default => ref _default;

        public StorageOptions(MemberTypes memberType,
            Encoding encoding,
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

            Encoding = encoding;
        }
    }
}
