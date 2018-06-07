using System.IO;
using DBClientFiles.NET.Internals.Segments;

namespace DBClientFiles.NET.Internals.Versions.Headers
{
    internal struct WDB2 : IFileHeader
    {
        public WDB2(BinaryReader reader)
        {
            Signature = Signatures.WDB2;

            RecordCount = reader.ReadInt32();
            FieldCount = reader.ReadInt32();
            RecordSize = reader.ReadInt32();
            StringTableLength = reader.ReadInt32();
            TableHash = reader.ReadUInt32();
            LayoutHash = reader.ReadUInt32();
            reader.BaseStream.Position += 4; // timestamp_last_written
            MinIndex = reader.ReadInt32();
            MaxIndex = reader.ReadInt32();
            reader.BaseStream.Position += 4; // locale
            CopyTableLength = reader.ReadInt32();

            if (MaxIndex != 0)
                reader.BaseStream.Position += (4 + 2) * (MaxIndex - MinIndex + 1);

            HasIndexTable = false;
            HasOffsetMap = false;

            IndexColumn = 0;
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
        public int MinIndex { get; }
        public int MaxIndex { get; }
        public int CopyTableLength { get; }

        public bool HasIndexTable { get; }
        public bool HasOffsetMap { get; }

        public int IndexColumn { get; }
    }
}