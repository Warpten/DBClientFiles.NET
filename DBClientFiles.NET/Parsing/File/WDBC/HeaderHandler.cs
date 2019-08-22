using System;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    internal sealed class HeaderHandler : AbstractHeaderHandler<Header>
    {
        public override int RecordCount => Structure.RecordCount;
        public override int FieldCount => Structure.FieldCount;
        public override int RecordSize => Structure.RecordSize;

        public override int IndexColumn { get; } = 0;

        private BlockReference _stringTableRef;

        public override ref readonly BlockReference StringTable => ref _stringTableRef;

        // Properties that don't exist
        public override int MaxIndex => throw new InvalidOperationException();
        public override int MinIndex => throw new InvalidOperationException();

        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
            _stringTableRef = new BlockReference(Structure.StringTableLength != 0, Structure.StringTableLength);
        }
    }
}
