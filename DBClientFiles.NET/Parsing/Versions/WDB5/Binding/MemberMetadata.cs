using DBClientFiles.NET.Parsing.Enums;
using DBClientFiles.NET.Parsing.Shared.Binding;

namespace DBClientFiles.NET.Parsing.Versions.WDB5.Binding
{
    internal sealed class MemberMetadata : BaseMemberMetadata
    {
        public override int Cardinality                       { get; internal set; } = -1;
        public override MemberMetadataProperties Properties   { get; internal set; } = 0;

        private CompressionData _compressionData;
        public override ref CompressionData CompressionData => ref _compressionData;

        public override T GetDefaultValue<T>() => default;
    }
}
