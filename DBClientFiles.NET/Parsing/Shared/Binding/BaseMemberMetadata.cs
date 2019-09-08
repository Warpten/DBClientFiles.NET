using DBClientFiles.NET.Parsing.Enums;
using System.Linq;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Parsing.Shared.Binding
{
    internal abstract class BaseMemberMetadata : IMemberMetadata
    {
        public abstract int Cardinality { get; internal set; }
        public abstract MemberMetadataProperties Properties { get; internal set; }

        public abstract T GetDefaultValue<T>() where T : unmanaged;

        // Stored as int because it nicely fits byte[4]
        public Variant<int> DefaultValue { get; internal set; }

        public abstract ref CompressionData CompressionData { get; }
        
        /// <summary>
        /// Offset of the field's value in the record segment.
        /// </summary>
        public int Offset { get; internal set; }

        /// <summary>
        /// Size of the field's value in the record segment.
        /// </summary>
        public int Size { get; internal set; }

        public override string ToString()
        {
            return $"Offset: {Offset} Size: {Size} Cardinality: {Cardinality} Properties: {Properties} Compression: {{ {CompressionData} }} DefaultValue: {{ {DefaultValue.Value:X8} }}";
        }
    }
}
