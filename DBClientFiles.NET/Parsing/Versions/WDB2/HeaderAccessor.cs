using DBClientFiles.NET.Parsing.Shared.Headers;
using DBClientFiles.NET.Parsing.Shared.Segments;

namespace DBClientFiles.NET.Parsing.Versions.WDB2
{
    internal sealed class HeaderAccessor : AbstractHeaderAccessor<Header>
    {
        public override int RecordCount { get; }
        public override int FieldCount { get; }
        public override int RecordSize { get; }
        public override int MaxIndex { get; }
        public override int MinIndex { get; }

        public override int IndexColumn { get; } = 0;

        private readonly SegmentReference _stringTableRef;
        public override ref readonly SegmentReference StringTable => ref _stringTableRef;

        public HeaderAccessor(in Header header) : base()
        {
            RecordCount = header.RecordCount;
            RecordSize = header.RecordSize;
            FieldCount = header.FieldCount;

            MinIndex = header.MinIndex;
            MaxIndex = header.MaxIndex;

            _stringTableRef = new SegmentReference(header.StringTableLength != 0, header.StringTableLength);
        }
    }
}
