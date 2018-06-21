using System.IO;
using DBClientFiles.NET.Internals.Segments;
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace DBClientFiles.NET.Internals.Versions.Headers
{
    internal struct WDBC : IFileHeader
    {
        public WDBC(BinaryReader reader)
        {
            Signature = Signatures.WDBC;

            RecordCount = reader.ReadInt32();
            FieldCount = reader.ReadInt32();
            RecordSize = reader.ReadInt32();
            StringTableLength = reader.ReadInt32();

            HasIndexTable = false;
            HasForeignIds = false;
            HasOffsetMap = false;

            CopyTableLength = 0;
            IndexColumn = 0;
            LayoutHash = 0;
            TableHash = 0;
            MinIndex = 0;
            MaxIndex = 0;
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
        public bool HasForeignIds { get; }
        public bool HasOffsetMap { get; }

        public int IndexColumn { get; }
    }
}