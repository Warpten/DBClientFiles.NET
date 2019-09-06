using DBClientFiles.NET.Parsing.Shared.Headers;
using DBClientFiles.NET.Parsing.Shared.Segments;
using System;

namespace DBClientFiles.NET.Parsing.Versions.WDBC
{
    internal sealed class HeaderAccessor : AbstractHeaderAccessor<Header>
    {
        public override int RecordCount => Header.RecordCount;
        public override int FieldCount => Header.FieldCount;
        public override int RecordSize => Header.RecordSize;

        private readonly SegmentReference _stringTableRef;

        public override ref readonly SegmentReference StringTable => ref _stringTableRef;

        // Not defined for WDBC
        public override int MaxIndex => throw new InvalidOperationException();
        public override int MinIndex => throw new InvalidOperationException();

        // WDBC always hsa index as first column
        public override int IndexColumn { get; } = 0;

        public HeaderAccessor(in Header header) : base(in header)
        {
            _stringTableRef = new SegmentReference(Header.StringTableLength != 0, Header.StringTableLength);
        }
    }
}
