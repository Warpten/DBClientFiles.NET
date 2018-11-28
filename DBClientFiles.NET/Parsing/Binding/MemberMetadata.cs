using DBClientFiles.NET.Parsing.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Parsing.Binding
{
    internal abstract class BaseMemberMetadata : IMemberMetadata
    {
        public abstract MemberCompressionType CompressionType { get; internal set; }
        public abstract uint CompressionIndex { get; internal set; }
        public abstract int Cardinality { get; internal set; }
        public abstract MemberMetadataProperties Properties { get; internal set; }

        public abstract uint Size { get; internal set; }
        public abstract uint Offset { get; internal set; }

        public abstract T GetDefaultValue<T>() where T : unmanaged;

        // This is the only way that I know of to have internal setters and public getters living side-by-side in interface definitions.
        MemberCompressionType IFileMemberMetadata.CompressionType => CompressionType;
        uint IFileMemberMetadata.CompressionIndex => CompressionIndex;
        int IFileMemberMetadata.Cardinality => Cardinality;
        MemberMetadataProperties IFileMemberMetadata.Properties => Properties;

        uint IFileMemberMetadata.Size => Size;
        uint IFileMemberMetadata.Offset => Offset;

        MemberCompressionType IWritableMemberMetadata.CompressionType { set { CompressionType = value; } }
        uint IWritableMemberMetadata.CompressionIndex { set { CompressionIndex = value; } }
        int IWritableMemberMetadata.Cardinality { set { Cardinality = value; } }
        MemberMetadataProperties IWritableMemberMetadata.Properties { set { Properties = value; } }

        uint IWritableMemberMetadata.Size { set { Size = value; } }
        uint IWritableMemberMetadata.Offset { set { Offset = value; } }
    }
}
