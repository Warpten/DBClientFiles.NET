using DBClientFiles.NET.Parsing.Shared.Headers;
using DBClientFiles.NET.Parsing.Shared.Segments;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBClientFiles.NET.Parsing.Versions.WDC1
{
    internal sealed class HeaderAccessor : AbstractHeaderAccessor<Header>
    {
        public override int RecordCount => Header.RecordCount;
        public override int FieldCount => Header.FieldCount;
        public override int RecordSize => Header.RecordSize;

        public override int IndexColumn => Header.IndexColumn;

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

        public override int MaxIndex => Header.MaxIndex;
        public override int MinIndex => Header.MinIndex;

        public HeaderAccessor(in Header header) : base(header)
        {
            _copyTableRef = new SegmentReference(Header.CopyTableSize != 0, Header.CopyTableSize);
            _palletDataRef = new SegmentReference(Header.PalletDataSize != 0, Header.PalletDataSize);

            _stringTableRef = new SegmentReference((Header.Flags & 0x01) == 0, Header.StringTableLength);
            _offsetMapRef = new SegmentReference((Header.Flags & 0x01) != 0,
                (Header.MaxIndex - Header.MinIndex + 1) * (4 + 2),
                Header.OffsetMapOffset);

            _indexTableRef = new SegmentReference(Header.IdListSize != 0, Header.IdListSize);

            _fieldInfoRef = new SegmentReference(true, Header.TotalFieldCount * (2 + 2));
            _extendedFieldInfoRef = new SegmentReference(true, Header.FieldStorageInfoSize);

            _relationshipTableRef = new SegmentReference(Header.RelationshipDataSize != 0, Header.RelationshipDataSize);
        }
    }
}
