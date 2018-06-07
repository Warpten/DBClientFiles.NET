using System.IO;
using DBClientFiles.NET.Internals.Segments;

namespace DBClientFiles.NET.Internals.Versions.Headers
{
    internal struct WDC1 : IFileHeader
    {
        public WDC1(BinaryReader reader)
        {
            Signature = Signatures.WDC1;

            RecordCount = reader.ReadInt32();
            FieldCount = reader.ReadInt32();
            RecordSize = reader.ReadInt32();
            StringTableLength = reader.ReadInt32();
            TableHash = reader.ReadUInt32();
            LayoutHash = reader.ReadUInt32();
            MinIndex = reader.ReadInt32();
            MaxIndex = reader.ReadInt32();
            reader.BaseStream.Position += 4; // locale
            CopyTableLength = reader.ReadInt32();
            var flags = reader.ReadInt16();
            IndexColumn = reader.ReadInt16();

            HasIndexTable = true;
            HasOffsetMap = (flags & 0x01) != 0;
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
        public bool HasOffsetMap { get; }

        public int IndexColumn { get; }
    }
}