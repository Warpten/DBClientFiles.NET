using System;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.File.WDB2
{
    internal sealed class HeaderHandler : AbstractHeaderHandler<Header>
    {
        public override int RecordCount => Structure.RecordCount;
        public override int FieldCount => Structure.FieldCount;
        public override int RecordSize => Structure.RecordSize;
        public override int MaxIndex => Structure.MaxIndex;
        public override int MinIndex => Structure.MinIndex;

        public override int IndexColumn { get; } = 0;

        private BlockReference _stringTableRef;
        public override ref readonly BlockReference StringTable => ref _stringTableRef;

        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
            _stringTableRef = new BlockReference(Structure.StringTableLength != 0, Structure.StringTableLength);
        }
    }
}
