using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.File.WDC1
{
    internal sealed class HeaderHandler : AbstractHeaderHandler<Header>
    {
        public override int RecordCount => Structure.RecordCount;
        public override int FieldCount => Structure.FieldCount;
        public override int RecordSize => Structure.RecordSize;
        public override int StringTableLength => Structure.StringTableLength;

        public override int IndexColumn => Structure.IndexColumn;

        public override bool HasIndexTable => (Structure.Flags & 0x04) == 0 && Structure.IdListSize > 0;
        public override bool HasOffsetMap => (Structure.Flags & 0x1) != 0;
        public override bool HasForeignIds { get; } = false;

        public override int MaxIndex => Structure.MaxIndex;
        public override int MinIndex => Structure.MinIndex;
        public override int CopyTableLength => Structure.CopyTableSize;

        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
        }
    }
}
