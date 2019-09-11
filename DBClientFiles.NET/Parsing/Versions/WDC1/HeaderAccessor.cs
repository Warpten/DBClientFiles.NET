using DBClientFiles.NET.Parsing.Shared.Headers;
using DBClientFiles.NET.Parsing.Shared.Segments;

namespace DBClientFiles.NET.Parsing.Versions.WDC1
{
    internal sealed class HeaderAccessor : AbstractHeaderAccessor<Header>
    {
        public override int RecordCount { get; }
        public override int FieldCount { get; }
        public override int RecordSize { get; }
        public override int MaxIndex { get; }
        public override int MinIndex { get; }

        public override int IndexColumn { get; }

        private readonly SegmentReference _copyTableRef;
        private readonly SegmentReference _commonDataRef;
        private readonly SegmentReference _palletDataRef;
        private readonly SegmentReference _stringTableRef;
        private readonly SegmentReference _offsetMapRef;
        private readonly SegmentReference _indexTableRef;
        private readonly SegmentReference _fieldInfoRef;
        private readonly SegmentReference _extendedFieldInfoRef;
        private readonly SegmentReference _relationshipTableRef;

        public override ref readonly SegmentReference StringTable => ref _stringTableRef;
        public override ref readonly SegmentReference Common => ref _commonDataRef;
        public override ref readonly SegmentReference CopyTable => ref _copyTableRef;
        public override ref readonly SegmentReference Pallet => ref _palletDataRef;
        public override ref readonly SegmentReference OffsetMap => ref _offsetMapRef;
        public override ref readonly SegmentReference IndexTable => ref _indexTableRef;
        public override ref readonly SegmentReference FieldInfo => ref _fieldInfoRef;
        public override ref readonly SegmentReference ExtendedFieldInfo => ref _extendedFieldInfoRef;
        public override ref readonly SegmentReference RelationshipTable => ref _relationshipTableRef;


        public HeaderAccessor(in Header header) : base()
        {
            RecordCount = header.RecordCount;
            RecordSize = header.RecordSize;
            FieldCount = header.FieldCount;

            MinIndex = header.MinIndex;
            MaxIndex = header.MaxIndex;

            IndexColumn = header.IndexColumn;

            _copyTableRef = new SegmentReference(header.CopyTableSize != 0, header.CopyTableSize);
            _palletDataRef = new SegmentReference(header.PalletDataSize != 0, header.PalletDataSize);

            _stringTableRef = new SegmentReference((header.Flags & 0x01) == 0, header.StringTableLength);
            _offsetMapRef = new SegmentReference((header.Flags & 0x01) != 0,
                (header.MaxIndex - header.MinIndex + 1) * (4 + 2),
                header.OffsetMapOffset);

            _indexTableRef = new SegmentReference(header.IdListSize != 0, header.IdListSize);

            _fieldInfoRef = new SegmentReference(true, header.TotalFieldCount * (2 + 2));
            _extendedFieldInfoRef = new SegmentReference(true, header.FieldStorageInfoSize);

            _relationshipTableRef = new SegmentReference(header.RelationshipDataSize != 0, header.RelationshipDataSize);
        }
    }
}
