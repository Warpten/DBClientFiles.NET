using System;
using System.Runtime.InteropServices;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.Versions.WDB5.Segments.Handlers
{
    internal sealed class HeaderHandler : AbstractHeaderHandler<Header>
    {
        public override int RecordCount => Structure.RecordCount;
        public override int FieldCount => Structure.FieldCount;
        public override int RecordSize => Structure.RecordSize;

        public override int IndexColumn { get; } = 0;

        private SegmentReference _stringTableRef;
        private SegmentReference _indexTable;
        private SegmentReference _offsetMap;
        private SegmentReference _fieldInfoRef;
        private SegmentReference _relationshipTableRef;
        private SegmentReference _copyTableRef;

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

        public override int MaxIndex => Structure.MaxIndex;
        public override int MinIndex => Structure.MinIndex;

        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
            _fieldInfoRef = new SegmentReference(true, Structure.FieldCount * (2 + 2));

            // String table and offset map are mutually exclusive.
            _stringTableRef = new SegmentReference((Structure.Flags & 0x01) == 0, Structure.StringTableLength);

            _offsetMap = new SegmentReference((Structure.Flags & 0x01) != 0, (Structure.MaxIndex - Structure.MinIndex + 1) * (4 + 2), Structure.StringTableLength);
            _relationshipTableRef = new SegmentReference((Structure.Flags & 0x02) != 0, (Structure.MaxIndex - Structure.MinIndex + 1) * 4);
            _indexTable = new SegmentReference((Structure.Flags & 0x04) != 0, Structure.RecordCount * 4);

            _copyTableRef = new SegmentReference(Structure.CopyTableLength != 0, Structure.CopyTableLength / (4 + 4));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Header
    {
        public readonly Signatures Signature;
        public readonly int RecordCount;
        public readonly int FieldCount;
        public readonly int RecordSize;
        public readonly int StringTableLength;
        public readonly uint TableHash;
        public readonly uint LayoutHash;
        public readonly int MinIndex;
        public readonly int MaxIndex;
        public readonly int Locale;
        public readonly int CopyTableLength;
        public readonly ushort Flags;
        public readonly short IndexColumn;
    }
}
