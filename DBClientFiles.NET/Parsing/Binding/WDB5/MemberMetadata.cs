using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.Enums;

namespace DBClientFiles.NET.Internals.Binding.Members.WDB5
{
    internal sealed class MemberMetadata : BaseMemberMetadata
    {
        public override MemberCompressionType CompressionType { get; internal set; } = MemberCompressionType.None;
        public override uint CompressionIndex                 { get; internal set; } = 0;
        public override int Cardinality                       { get; internal set; } = -1;
        public override MemberMetadataProperties Properties   { get; internal set; } = 0;

        public override uint Size { get; internal set; }
        public override uint Offset { get; internal set; }

        public override T GetDefaultValue<T>()
        {
            return default;
        }
    }
}
