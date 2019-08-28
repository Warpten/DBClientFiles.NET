using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.Versions.WDC1.Segments.Handlers
{
    internal sealed class HeaderHandler : AbstractHeaderHandler<Header>
    {
        public override int RecordCount => Structure.RecordCount;
        public override int FieldCount => Structure.FieldCount;
        public override int RecordSize => Structure.RecordSize;

        public override int IndexColumn => Structure.IndexColumn;

        private SegmentReference _copyTableRef;
        private SegmentReference _commonDataRef;
        private SegmentReference _palletDataRef;
        private SegmentReference _stringTableRef;
        private SegmentReference _offsetMapRef;
        private SegmentReference _indexTableRef;
        private SegmentReference _fieldInfoRef;
        private SegmentReference _extendedFieldInfoRef;
        private SegmentReference _relationshipTableRef;

        public override ref readonly SegmentReference StringTable       => ref _stringTableRef;
        public override ref readonly SegmentReference Common            => ref _commonDataRef;
        public override ref readonly SegmentReference CopyTable         => ref _copyTableRef;
        public override ref readonly SegmentReference Pallet            => ref _palletDataRef;
        public override ref readonly SegmentReference OffsetMap         => ref _offsetMapRef;
        public override ref readonly SegmentReference IndexTable        => ref _indexTableRef;
        public override ref readonly SegmentReference FieldInfo         => ref _fieldInfoRef;
        public override ref readonly SegmentReference ExtendedFieldInfo => ref _extendedFieldInfoRef;
        public override ref readonly SegmentReference RelationshipTable => ref _relationshipTableRef;

        public override int MaxIndex => Structure.MaxIndex;
        public override int MinIndex => Structure.MinIndex;

        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
            _copyTableRef = new SegmentReference(Structure.CopyTableSize != 0, Structure.CopyTableSize);
            _palletDataRef = new SegmentReference(Structure.PalletDataSize != 0, Structure.PalletDataSize);

            _stringTableRef = new SegmentReference((Structure.Flags & 0x01) == 0, Structure.StringTableLength);
            _offsetMapRef = new SegmentReference((Structure.Flags & 0x01) != 0,
                (Structure.MaxIndex - Structure.MinIndex + 1) * (4 + 2),
                Structure.OffsetMapOffset);

            _indexTableRef = new SegmentReference(Structure.IdListSize != 0, Structure.IdListSize);

            _fieldInfoRef = new SegmentReference(true, Structure.TotalFieldCount * (2 + 2));
            _extendedFieldInfoRef = new SegmentReference(true, Structure.FieldStorageInfoSize);

            _relationshipTableRef = new SegmentReference(Structure.RelationshipDataSize != 0, Structure.RelationshipDataSize);
        }
    }

    /// <summary>
    /// Representation of a WDBC header.
    ///
    /// See <a href="http://www.wowdev.wiki/DBC">the wiki</a>.
    /// </summary>
    internal readonly struct Header
    {
        public readonly Signatures Signature;
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
    }
}
