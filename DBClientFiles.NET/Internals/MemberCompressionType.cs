namespace DBClientFiles.NET.Internals
{
    public enum MemberCompressionType
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
