using DBClientFiles.NET.Parsing.Shared.Headers;
using DBClientFiles.NET.Parsing.Shared.Segments;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Parsing.Versions.WDB5
{
    internal sealed class HeaderAccessor : AbstractHeaderAccessor<Header>
    {
        public override int RecordCount => Header.RecordCount;
        public override int FieldCount => Header.FieldCount;
        public override int RecordSize => Header.RecordSize;

        public override int IndexColumn => Header.IndexColumn;

        private readonly SegmentReference _stringTableRef;
        private readonly SegmentReference _indexTable;
        private readonly SegmentReference _offsetMap;
        private readonly SegmentReference _fieldInfoRef;
        private readonly SegmentReference _relationshipTableRef;
        private readonly SegmentReference _copyTableRef;

        public override ref readonly SegmentReference StringTable => ref _stringTableRef;
        public override ref readonly SegmentReference IndexTable => ref _indexTable;
        public override ref readonly SegmentReference OffsetMap => ref _offsetMap;
        public override ref readonly SegmentReference FieldInfo => ref _fieldInfoRef;
        public override ref readonly SegmentReference RelationshipTable => ref _relationshipTableRef;
        public override ref readonly SegmentReference CopyTable => ref _copyTableRef;

        // Blocks this file does not have
        public override ref readonly SegmentReference Common => throw new NotImplementedException();
        public override ref readonly SegmentReference Pallet => throw new NotImplementedException();
        public override ref readonly SegmentReference ExtendedFieldInfo => throw new NotImplementedException();

        public override int MaxIndex => Header.MinIndex;
        public override int MinIndex => Header.MaxIndex;

        public HeaderAccessor(in Header header) : base(in header)
        {
            // Just give me static_assert god dammit
            Debug.Assert(Unsafe.SizeOf<Header>() == 4 * 11 + 2 * 2, "umpf");

            _fieldInfoRef = new SegmentReference(true, Header.FieldCount * (2 + 2));

            // String table and offset map are mutually exclusive.
            _stringTableRef = new SegmentReference((Header.Flags & 0x01) == 0,
                Header.StringTableLength);

            _offsetMap = new SegmentReference((Header.Flags & 0x01) != 0,
                (Header.MaxIndex - Header.MinIndex + 1) * (4 + 2),
                Header.StringTableLength);
            _relationshipTableRef = new SegmentReference((Header.Flags & 0x02) != 0,
                (Header.MaxIndex - Header.MinIndex + 1) * 4);
            _indexTable = new SegmentReference((Header.Flags & 0x04) != 0,
                Header.RecordCount * 4);

            _copyTableRef = new SegmentReference(Header.CopyTableLength != 0,
                Header.CopyTableLength / (4 + 4));
        }
    }
}
