using DBClientFiles.NET.Parsing.Shared.Headers;
using System.IO;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.Versions.WDC1
{
    /// <summary>
    /// Representation of a WDBC header.
    ///
    /// See <a href="http://www.wowdev.wiki/DBC">the wiki</a>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Header : IHeader
    {
        public readonly int RecordCount;
        public readonly int FieldCount;
        public readonly int RecordSize;
        public readonly int StringTableLength;
        public readonly int TableHash;
        public readonly int LayoutHash;
        public readonly int MinIndex;
        public readonly int MaxIndex;
        public readonly int Locale;
        public readonly int CopyTableSize;
        public readonly short Flags;
        public readonly short IndexColumn;
        public readonly int TotalFieldCount;     // from WDC1 onwards, this value seems to always be the same as the 'field_count' value
        public readonly int BitpackedDataOffset; // relative position in record where bitpacked data begins; not important for parsing the file
        public readonly int LookupColumnCount;
        public readonly int OffsetMapOffset;     // Offset to array of struct {uint32_t offset; uint16_t size;}[max_id - min_id + 1];
        public readonly int IdListSize;          // List of ids present in the DB file
        public readonly int FieldStorageInfoSize;
        public readonly int CommonDataSize;
        public readonly int PalletDataSize;
        public readonly int RelationshipDataSize;

        public IBinaryStorageFile<T> MakeStorageFile<T>(in StorageOptions options, Stream dataStream)
            => new StorageFile<T>(in options, in this, dataStream);
    }
}
