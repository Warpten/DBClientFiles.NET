namespace DBClientFiles.NET.Internals
{
    internal enum MemberCompressionType
    {
        None,
        Immediate,
        CommonData,
        BitpackedPalletData,
        BitpackedPalletArrayData,
        SignedImmediate,

        // Not an actual compression type, here for convenience
        RelationshipData = 10
    }
}
