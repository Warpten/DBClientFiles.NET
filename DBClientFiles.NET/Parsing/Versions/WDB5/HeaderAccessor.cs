using DBClientFiles.NET.Parsing.Shared.Headers;
using DBClientFiles.NET.Parsing.Shared.Segments;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Parsing.Versions.WDB5
{
    internal sealed class HeaderAccessor : AbstractHeaderAccessor<Header>
    {
        public override int RecordCount { get; }
        public override int FieldCount { get; }
        public override int RecordSize { get; }
        public override int MaxIndex { get; }
        public override int MinIndex { get; }

        public override int IndexColumn { get; } 


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


        public HeaderAccessor(in Header header) : base()
        {
            RecordCount = header.RecordCount;
            RecordSize = header.RecordSize;
            FieldCount = header.FieldCount;

            MinIndex = header.MinIndex;
            MaxIndex = header.MaxIndex;

            IndexColumn = header.IndexColumn;

            _fieldInfoRef = new SegmentReference(true, header.FieldCount * (2 + 2));

            // String table and offset map are mutually exclusive.
            _stringTableRef = new SegmentReference((header.Flags & 0x01) == 0,
                header.StringTableLength);

            _offsetMap = new SegmentReference((header.Flags & 0x01) != 0,
                (header.MaxIndex - header.MinIndex + 1) * (4 + 2),
                header.StringTableLength);

            _relationshipTableRef = new SegmentReference((header.Flags & 0x02) != 0, (header.MaxIndex - header.MinIndex + 1) * 4);
            _indexTable           = new SegmentReference((header.Flags & 0x04) != 0, header.RecordCount * 4);
            _copyTableRef         = new SegmentReference(header.CopyTableLength != 0, header.CopyTableLength / (4 + 4));
        }
    }
}
