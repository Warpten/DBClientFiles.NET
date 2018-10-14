using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    /// <summary>
    /// Representation of a WDBC header.
    ///
    /// See <a href="http://www.wowdev.wiki/DBC">the wiki</a>.
    /// </summary>
    internal class Header : IFileHeader
    {
        public int Size => UnsafeCache<Header>.Size + 4;
        public Signatures Signature => Signatures.WDBC;

        public uint TableHash => throw new NotImplementedException();
        public uint LayoutHash => throw new NotImplementedException();

        public uint RecordSize { get; set; }
        public uint RecordCount { get; set; }
        public uint FieldCount { get; set; }
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
            RecordCount = reader.ReadUInt32();
            FieldCount = reader.ReadUInt32();
            RecordSize = reader.ReadUInt32();
            StringTableLength = reader.ReadInt32();
        }
    }
}
