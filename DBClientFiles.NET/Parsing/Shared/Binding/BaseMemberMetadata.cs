using DBClientFiles.NET.Parsing.Enums;

namespace DBClientFiles.NET.Parsing.Shared.Binding
{
    internal abstract class BaseMemberMetadata : IMemberMetadata
    {
        public abstract int Cardinality { get; internal set; }
        public abstract MemberMetadataProperties Properties { get; internal set; }

        public abstract T GetDefaultValue<T>() where T : unmanaged;
        // Default value stored as a byte blob
        public byte[] RawDefaultValue { get; internal set; }

        public abstract ref CompressionData CompressionData { get; } 
    }
}
