using DBClientFiles.NET.Parsing.Shared.Headers;
using DBClientFiles.NET.Parsing.Shared.Segments;

namespace DBClientFiles.NET.Parsing.Versions.WDB2
{
    internal sealed class HeaderAccessor : AbstractHeaderAccessor<Header>
    {
        public override int RecordCount => Header.RecordCount;
        public override int FieldCount => Header.FieldCount;
        public override int RecordSize => Header.RecordSize;
        public override int MaxIndex => Header.MaxIndex;
        public override int MinIndex => Header.MinIndex;

        public override int IndexColumn { get; } = 0;

        private readonly SegmentReference _stringTableRef;
        public override ref readonly SegmentReference StringTable => ref _stringTableRef;

        public HeaderAccessor(in Header source) : base(in source)
        {
            _stringTableRef = new SegmentReference(Header.StringTableLength != 0, Header.StringTableLength);
        }
    }
}
