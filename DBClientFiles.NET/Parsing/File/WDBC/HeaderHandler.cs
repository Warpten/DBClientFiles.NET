using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    internal sealed class HeaderHandler : AbstractHeaderHandler<Header>
    {
        public override int RecordCount => Structure.RecordCount;
        public override int FieldCount => Structure.FieldCount;
        public override int RecordSize => Structure.RecordSize;
        public override int StringTableLength => Structure.StringTableLength;

        public override int IndexColumn { get; } = 0;

        public override bool HasIndexTable { get; } = false;
        public override bool HasOffsetMap { get; } = false;
        public override bool HasForeignIds { get; } = false;

        public override int MaxIndex => throw new InvalidOperationException();
        public override int MinIndex => throw new InvalidOperationException();
        public override int CopyTableLength => throw new InvalidOperationException();


        public HeaderHandler(BinaryReader source) : base(source)
        {
        }
    }
}
