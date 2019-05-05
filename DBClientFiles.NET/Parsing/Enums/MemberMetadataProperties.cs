using System;

namespace DBClientFiles.NET.Parsing.Enums
{
    [Flags]
    public enum MemberMetadataProperties : byte
    {
        /// <summary>
        /// This member is the record's index.
        /// </summary>
        Index = 0x01,
        /// <summary>
        /// This member's value is signed.
        /// </summary>
        Signed = 0x02,
    }
}
