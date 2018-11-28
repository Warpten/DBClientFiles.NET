using DBClientFiles.NET.Utils;
using System;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.WDB5
{
    internal readonly struct Header : IFileHeader
    {
        public int Size => UnsafeCache<Header>.Size;
        public Signatures Signature => Signatures.WDB5;

        public int RecordCount { get; }
        public int FieldCount { get;  }
        public int RecordSize { get;  }
        public int StringTableLength { get; }
        public uint TableHash { get; }
        public uint LayoutHash { get; }
        public int MinIndex { get; }
        public int MaxIndex { get; }
        public int CopyTableLength { get; }
        private ushort Flags { get; }
        public short IndexColumn { get; }

        public bool HasIndexTable => (Flags & 0x04) != 0;
        public bool HasOffsetMap => (Flags & 0x01) != 0;
        // HasRelationshipID 0x02; unsure if needed

        public Header(BinaryReader reader)
        {
            RecordCount = reader.ReadInt32();
            FieldCount = reader.ReadInt32();
            RecordSize = reader.ReadInt32();
            StringTableLength = reader.ReadInt32();

            TableHash = reader.ReadUInt32();
            LayoutHash = reader.ReadUInt32();

            MinIndex = reader.ReadInt32();
            MaxIndex = reader.ReadInt32();

            CopyTableLength = reader.ReadInt32();

            Flags = reader.ReadUInt16();
            IndexColumn = reader.ReadInt16();
        }

        public bool HasForeignIds => (Flags & 0x02) != 0;
    }
}
