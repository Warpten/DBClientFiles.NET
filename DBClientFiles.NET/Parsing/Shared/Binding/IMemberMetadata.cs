using DBClientFiles.NET.Parsing.Enums;

namespace DBClientFiles.NET.Parsing.Shared.Binding
{
    public struct CompressionData
    {
        /// <summary>
        /// Compression type of the field in the record
        /// </summary>
        public MemberCompressionType Type { get; internal set; }
        
        /// <summary>
        /// Offset of the field in the record.
        /// </summary>
        public int Offset { get; internal set; }

        /// <summary>
        /// Size of the field in the record.
        /// </summary>
        public int Size { get; internal set; }

        /// <summary>
        /// Size of the field's compressed data in the corresponding block.
        /// </summary>
        public int CompressedDataSize { get; internal set; }

        public int CompressionDataOffset { get; internal set; }

        public override string ToString()
        {
            return $"Type: {Type} Offset: {Offset} Size: {Size}";
        }
    }

    /// <summary>
    /// This is the basic interface used when reading member metadata from game files.
    /// </summary>
    public interface IMemberMetadata
    {
        /// <summary>
        /// Compression information for the field, extracted from the files.
        /// </summary>
        ref CompressionData CompressionData { get; }

        /// <summary>
        /// If this member is an array, this is the size of said array.
        ///
        /// The default "this-is-not-an-array" value should be 0.
        /// The default "i-do-not-know" value should be -1.
        /// </summary>
        int Cardinality { get; }

        /// <summary>
        /// This is a bitmask of various properties that may be defined by the file.
        /// </summary>
        MemberMetadataProperties Properties { get; }

        /// <summary>
        /// Provide a default value for the member.
        /// </summary>
        /// <typeparam name="T">The type of the member.</typeparam>
        /// <returns>An instance of <see cref="{T}"/>.</returns>
        /// <remarks>A default implementation returns <c>default(T)</c>.</remarks>
        T GetDefaultValue<T>() where T : unmanaged;
    }
}
