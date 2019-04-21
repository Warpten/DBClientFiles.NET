using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.File.WDB2
{
    internal sealed class HeaderHandler : AbstractHeaderHandler<Header>
    {
        public override int RecordCount => Structure.RecordCount;
        public override int FieldCount => Structure.FieldCount;
        public override int RecordSize => Structure.RecordSize;
        public override int StringTableLength => Structure.StringTableLength;
        public override int MaxIndex => Structure.MaxIndex;
        public override int MinIndex => Structure.MinIndex;

        public override int IndexColumn { get; } = 0;

        public override bool HasIndexTable => throw new InvalidOperationException();
        public override bool HasOffsetMap => throw new InvalidOperationException();
        public override bool HasForeignIds => throw new InvalidOperationException();

        public override int CopyTableLength => throw new InvalidOperationException();
        
        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
        }
    }
}
