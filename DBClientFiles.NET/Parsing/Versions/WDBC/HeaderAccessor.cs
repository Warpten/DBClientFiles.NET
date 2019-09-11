using DBClientFiles.NET.Parsing.Shared.Headers;
using DBClientFiles.NET.Parsing.Shared.Segments;
using System;

namespace DBClientFiles.NET.Parsing.Versions.WDBC
{
    internal sealed class HeaderAccessor : AbstractHeaderAccessor<Header>
    {
        public override int RecordCount { get; }
        public override int FieldCount { get; }
        public override int RecordSize { get; }

        private readonly SegmentReference _stringTableRef;

        public override ref readonly SegmentReference StringTable => ref _stringTableRef;

        // Not defined for WDBC
        public override int MaxIndex => throw new InvalidOperationException();
        public override int MinIndex => throw new InvalidOperationException();

        // WDBC always has index as first column
        public override int IndexColumn { get; } = 0;

        public HeaderAccessor(in Header header) : base()
        {
            RecordCount = header.RecordCount;
            RecordSize = header.RecordSize;
            FieldCount = header.FieldCount;

            _stringTableRef = new SegmentReference(header.StringTableLength != 0, header.StringTableLength);
        }
    }
}
