using System.Runtime.InteropServices;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.Enums;

namespace DBClientFiles.NET.Parsing.File.WDC1
{
    internal sealed class MemberMetadata : BaseMemberMetadata
    {
        public override int Cardinality                       { get; internal set; } = -1;
        public override MemberMetadataProperties Properties   { get; internal set; } = 0;

        public override uint Size { get; internal set; }
        public override uint Offset { get; internal set; }

        private CompressionData _compressionData;
        public override ref CompressionData CompressionData => ref _compressionData;

        public override T GetDefaultValue<T>() => MemoryMarshal.Read<T>(RawDefaultValue);
    }
}
