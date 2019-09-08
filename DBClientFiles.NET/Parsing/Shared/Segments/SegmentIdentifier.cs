namespace DBClientFiles.NET.Parsing.Shared.Segments
{
    internal enum SegmentIdentifier : uint
    {
        Header            = 0x00000001,
        StringBlock       = 0x00000002,
        CopyTable         = 0x00000004,
        OffsetMap         = 0x00000008,
        IndexTable        = 0x00000010,
        FieldInfo         = 0x00000020,
        ExtendedFieldInfo = 0x00000040,
        CommonDataTable   = 0x00000080,
        PalletTable       = 0x00000100,
        RelationshipTable = 0x00000200,

        Ignored           = 0x80000000,


        Records           = 0xFFFFFFFF,
    }
}
