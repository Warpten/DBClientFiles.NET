using DBClientFiles.NET.Utils;
using System;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    /// <summary>
    /// Representation of a WDBC header.
    ///
    /// See <a href="http://www.wowdev.wiki/DBC">the wiki</a>.
    /// </summary>
    internal struct Header : IFileHeader
    {
        public int Size             => UnsafeCache<Header>.Size;
        public Signatures Signature => Signatures.WDBC;

        public int RecordSize        { get; private set; }
        public int RecordCount       { get; private set; }
        public int FieldCount        { get; private set; }
        public int StringTableLength { get; private set; }

        public void Read(BinaryReader reader)
        {
            RecordCount = reader.ReadInt32();
            FieldCount = reader.ReadInt32();
            RecordSize = reader.ReadInt32();
            StringTableLength = reader.ReadInt32();
        }

        public uint TableHash => throw new NotImplementedException();
        public uint LayoutHash => throw new NotImplementedException();
        public int MinIndex => throw new NotImplementedException();
        public int MaxIndex => throw new NotImplementedException();
        public int CopyTableLength => throw new NotImplementedException();
        public short IndexColumn => throw new NotImplementedException();
        public bool HasIndexTable => throw new NotImplementedException();
        public bool HasForeignIds => throw new NotImplementedException();
        public bool HasOffsetMap => throw new NotImplementedException();
    }
}
