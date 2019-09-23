namespace DBClientFiles.NET.Parsing.Enums
{
    public enum MemberCompressionType : uint
    {
        None,
        Immediate,
        CommonData,
        BitpackedPalletData,
        BitpackedPalletArrayData,
        SignedImmediate,

        // Not an actual compression type, here for convenience
        RelationshipData = 0xFFFFFFFE,
        Unknown = 0xFFFFFFFF,
    }
}
