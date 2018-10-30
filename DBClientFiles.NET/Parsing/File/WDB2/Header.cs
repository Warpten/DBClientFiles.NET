using DBClientFiles.NET.Utils;
using System;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.WDB2
{
    /// <summary>
    /// Representation of a WDB2 header.
    ///
    /// See <a href="http://www.wowdev.wiki/DBC">the wiki</a>.
    /// </summary>
    internal struct Header : IFileHeader
    {
        public int Size             => UnsafeCache<Header>.Size + 3 * 4;
        public Signatures Signature => Signatures.WDB2;

        public int RecordCount       { get; private set; }
        public int FieldCount        { get; private set; }
        public int RecordSize        { get; private set; }
        public int StringTableLength { get; private set; }
        public uint TableHash        { get; private set; }
        public uint LayoutHash       { get; private set; }
        public int MinIndex          { get; private set; }
        public int MaxIndex          { get; private set; }

        public void Read(BinaryReader reader)
        {
            RecordCount = reader.ReadInt32();
            FieldCount = reader.ReadInt32();
            RecordSize = reader.ReadInt32();
            StringTableLength = reader.ReadInt32();

            TableHash = reader.ReadUInt32();
            LayoutHash = reader.ReadUInt32();

            reader.BaseStream.Position += 4; // timestamp last written

            MinIndex = reader.ReadInt32();
            MaxIndex = reader.ReadInt32();

            reader.BaseStream.Position += 4 + 4; // locale + copy-table-size (which is always 0)
        }

        public int CopyTableLength => throw new NotImplementedException();
        public int IndexColumn => throw new NotImplementedException();
        public bool HasIndexTable => throw new NotImplementedException();
        public bool HasForeignIds => throw new NotImplementedException();
        public bool HasOffsetMap => throw new NotImplementedException();

    }
}
