using System.IO;
using DBClientFiles.NET.Internals.Segments;

namespace DBClientFiles.NET.Internals.Versions.Headers
{
    internal struct WDC2 : IFileHeader
    {
        public WDC2(BinaryReader reader)
        {
            Signature = Signatures.WDC2;

            RecordCount = reader.ReadInt32();
            FieldCount = reader.ReadInt32();
            RecordSize = reader.ReadInt32();
            StringTableLength = reader.ReadInt32();
            TableHash = reader.ReadUInt32();
            LayoutHash = reader.ReadUInt32();
            MinIndex = reader.ReadInt32();
            MaxIndex = reader.ReadInt32();
            reader.BaseStream.Position += 4; // locale
            var flags = reader.ReadInt16();
            IndexColumn = reader.ReadInt16();

            HasIndexTable = (flags & 0x04) != 0;
			HasForeignIds = (flags & 0x02) != 0;
            HasOffsetMap = (flags & 0x01) != 0;

            CopyTableLength = 0;
        }

        #region IStorage
        public Signatures Signature { get; }
        public uint TableHash { get; }
        public uint LayoutHash { get; }
        #endregion

        public int RecordSize { get; }
        public int RecordCount { get; }
        public int FieldCount { get; }

        public int StringTableLength { get; }
        public int CopyTableLength { get; }

        public int MinIndex { get; }
        public int MaxIndex { get; }

        public bool HasIndexTable { get; }
		public bool HasForeignIds { get; }
		public bool HasOffsetMap { get; }

        public int IndexColumn { get; }
    }
}