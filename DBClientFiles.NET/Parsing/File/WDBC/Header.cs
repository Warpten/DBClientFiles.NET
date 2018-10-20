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
    internal class Header : IFileHeader
    {
        public int Size => UnsafeCache<Header>.Size;
        public Signatures Signature => Signatures.WDBC;

        public uint TableHash => throw new NotImplementedException();
        public uint LayoutHash => throw new NotImplementedException();

        public int RecordSize { get; set; }
        public int RecordCount { get; set; }
        public int FieldCount { get; set; }
        public int StringTableLength { get; set; }

        public int MinIndex => throw new NotImplementedException();
        public int MaxIndex => throw new NotImplementedException();
        public int CopyTableLength => throw new NotImplementedException();
        public int IndexColumn => throw new NotImplementedException();
        public bool HasIndexTable => throw new NotImplementedException();
        public bool HasForeignIds => throw new NotImplementedException();
        public bool HasOffsetMap => throw new NotImplementedException();

        public void Read(BinaryReader reader)
        {
            RecordCount = reader.ReadInt32();
            FieldCount = reader.ReadInt32();
            RecordSize = reader.ReadInt32();
            StringTableLength = reader.ReadInt32();
        }
    }
}
