using DBClientFiles.NET.Parsing.Enums;
using System;

namespace DBClientFiles.NET.Parsing.Binding
{
    /// <summary>
    /// This is the basic interface used when reading member metadata from game files.
    /// </summary>
    public interface IFileMemberMetadata
    {
        /// <summary>
        /// The size of the member, be it in bytes, bits, and/or an expression of an element's size if an array or not.
        ///
        /// All the above are controlled by the caller and the caller only.
        /// </summary>
        uint Size { get; }

        /// <summary>
        /// See <see cref="Size"/>.
        /// </summary>
        uint Offset { get; }

        /// <summary>
        /// The type of compression used for this member. Default value should always be <see cref="MemberCompressionType.None"/>.
        /// </summary>
        MemberCompressionType CompressionType { get; }

        /// <summary>
        /// This is the index of this member in respect to other members with the same compression type.
        /// It is usually ignored for <see cref="MemberCompressionType.None"/>.
        /// </summary>
        uint CompressionIndex { get; }

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

    /// <summary>
    /// A copy of the above that exposes the same properties, but writable. This is the only way to have internally accessible setters.
    /// </summary>
    internal interface IWritableMemberMetadata
    {
        uint Size { set; }
        uint Offset { set; }
        MemberCompressionType CompressionType { set; }
        uint CompressionIndex { set; }
        int Cardinality { set; }
        MemberMetadataProperties Properties { set; }
    }

    internal interface IMemberMetadata : IFileMemberMetadata, IWritableMemberMetadata
    {
    }
}
