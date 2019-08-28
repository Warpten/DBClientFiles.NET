using System;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.Versions.WDBC
{
    internal sealed class HeaderHandler : AbstractHeaderHandler<Header>
    {
        public override int RecordCount => Structure.RecordCount;
        public override int FieldCount => Structure.FieldCount;
        public override int RecordSize => Structure.RecordSize;

        public override int IndexColumn { get; } = 0;

        private SegmentReference _stringTableRef;

        public override ref readonly SegmentReference StringTable => ref _stringTableRef;

        // Properties that don't exist
        public override int MaxIndex => throw new InvalidOperationException();
        public override int MinIndex => throw new InvalidOperationException();

        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
            _stringTableRef = new SegmentReference(Structure.StringTableLength != 0, Structure.StringTableLength);
        }
    }
}
