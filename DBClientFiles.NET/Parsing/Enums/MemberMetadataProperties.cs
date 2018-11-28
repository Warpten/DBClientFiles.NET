using System;

namespace DBClientFiles.NET.Parsing.Enums
{
    [Flags]
    public enum MemberMetadataProperties
    {
        /// <summary>
        /// This member is the record's index.
        /// </summary>
        Index,
        /// <summary>
        /// This member's value is signed.
        /// </summary>
        Signed,
    }
}
