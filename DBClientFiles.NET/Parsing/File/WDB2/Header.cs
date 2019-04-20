using DBClientFiles.NET.Utils;
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
        public int Size             => UnsafeCache<Header>.Size + 2 * 4;
        public Signatures Signature => Signatures.WDB2;

        public int RecordCount       { get; }
        public int FieldCount        { get; }
        public int RecordSize        { get; }
        public int StringTableLength { get; }
        public uint TableHash        { get; }
        public uint LayoutHash       { get; }
        // TimestampLastWritten
        public int MinIndex          { get; }
        public int MaxIndex          { get; }
        // Locale
        public int CopyTableLength   { get; }

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

            reader.BaseStream.Seek(4, SeekOrigin.Current); // locale + copy-table-size (which is always 0)

            CopyTableLength = reader.ReadInt32();
        }

        public short IndexColumn => 0;
        public bool HasIndexTable => false;
        public bool HasForeignIds => false;
        public bool HasOffsetMap => false;

    }
}
