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
    internal readonly struct Header : IFileHeader
    {
        public int Size             => UnsafeCache<Header>.Size + 3 * 4;
        public Signatures Signature => Signatures.WDB2;

        public int RecordCount       { get; }
        public int FieldCount        { get; }
        public int RecordSize        { get; }
        public int StringTableLength { get; }
        public uint TableHash        { get; }
        public uint LayoutHash       { get; }
        public int MinIndex          { get; }
        public int MaxIndex          { get; }

        public Header(BinaryReader reader)
        {
            RecordCount = reader.ReadInt32();
            FieldCount = reader.ReadInt32();
            RecordSize = reader.ReadInt32();
            StringTableLength = reader.ReadInt32();

            TableHash = reader.ReadUInt32();
            LayoutHash = reader.ReadUInt32();

            reader.BaseStream.Seek(4, SeekOrigin.Current); // timestamp last written

            MinIndex = reader.ReadInt32();
            MaxIndex = reader.ReadInt32();

            reader.BaseStream.Seek(4 + 4, SeekOrigin.Current); // locale + copy-table-size (which is always 0)
        }

        public int CopyTableLength => throw new NotImplementedException();
        public short IndexColumn => throw new NotImplementedException();
        public bool HasIndexTable => throw new NotImplementedException();
        public bool HasForeignIds => throw new NotImplementedException();
        public bool HasOffsetMap => throw new NotImplementedException();

    }
}
