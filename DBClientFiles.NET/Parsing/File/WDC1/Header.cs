using DBClientFiles.NET.Utils;
using System;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.WDC1
{
    /// <summary>
    /// Representation of a WDBC header.
    ///
    /// See <a href="http://www.wowdev.wiki/DBC">the wiki</a>.
    /// </summary>
    internal readonly struct Header : IFileHeader
    {
        public int Size             => UnsafeCache<Header>.Size;
        public Signatures Signature => Signatures.WDBC;

        public int RecordSize        { get; }
        public int RecordCount       { get; }
        public int FieldCount        { get; }
        public int StringTableLength { get; }

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

            var locale = reader.ReadUInt32();

            CopyTableLength = reader.ReadInt32();

            var flags = reader.ReadInt16();

            IndexColumn = reader.ReadInt16();
        }

        public uint TableHash { get; }
        public uint LayoutHash { get; }
        public int MinIndex { get; }
        public int MaxIndex { get; }
        public int CopyTableLength { get; }
        public short IndexColumn { get; }
        public bool HasIndexTable => false;
        public bool HasForeignIds => false;
        public bool HasOffsetMap => false;
    }
}
