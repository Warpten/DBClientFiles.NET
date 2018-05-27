namespace DBClientFiles.NET.Internals
{
    internal enum MemberCompressionType
    {
        None,
        Immediate,
        CommonData,
        BitpackedPalletData,
        BitpackedPalletArrayData,
        RelationshipData
    }
}
