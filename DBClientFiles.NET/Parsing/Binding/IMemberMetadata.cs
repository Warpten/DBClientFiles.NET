using DBClientFiles.NET.Parsing.Enums;

namespace DBClientFiles.NET.Parsing.Binding
{
    public struct CompressionData
    {
        public MemberCompressionType Type { get; internal set; }
        public int Offset { get; internal set; }
        public int Size { get; internal set; }
    }

    /// <summary>
    /// This is the basic interface used when reading member metadata from game files.
    /// </summary>
    public interface IMemberMetadata
    {
        /// <summary>
        /// The size of the member, in bits. If the corresponding member is an array, this should be the bit size
        /// of an element of said array.
        /// </summary>
        uint Size { get; }

        /// <summary>
        /// The offset, in bits, of this member from the start of the record.
        /// </summary>
        uint Offset { get; }

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
