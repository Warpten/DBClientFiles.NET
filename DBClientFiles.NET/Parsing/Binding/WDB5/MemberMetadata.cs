using DBClientFiles.NET.Parsing.Enums;

namespace DBClientFiles.NET.Internals.Binding.Members.WDB5
{
    internal sealed class MemberMetadata : IMemberMetadata
    {
        private uint _bitOffset;
        private uint _elementBitSize;

        public MemberMetadata(uint bitOffset, uint elementBitSize)
        {
            _bitOffset = bitOffset;
            _elementBitSize = elementBitSize;
        }

        public MemberCompressionType CompressionType { get; } = MemberCompressionType.None;
        public uint CompressionIndex                 { get; } = 0;
        public int Cardinality                       { get; } = -1;
        public MemberMetadataProperties Properties   { get; } = 0;

        public T GetDefaultValue<T>() => default;

        public uint GetElementBitSize()
        {
            return _elementBitSize;
        }

        public uint GetBitOffset()
        {
            return _bitOffset;
        }
    }
}
