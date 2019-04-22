using System;
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

        public override bool HasForeignIds => throw new InvalidOperationException();

        private BlockReference _stringTableRef;

        public override ref readonly BlockReference StringTable => ref _stringTableRef;

        // Blocks that don't exist
        public override ref readonly BlockReference OffsetMap         => throw new NotImplementedException();
        public override ref readonly BlockReference IndexTable        => throw new NotImplementedException();
        public override ref readonly BlockReference Pallet            => throw new NotImplementedException();
        public override ref readonly BlockReference Common            => throw new NotImplementedException();
        public override ref readonly BlockReference CopyTable         => throw new NotImplementedException();
        public override ref readonly BlockReference FieldInfo         => throw new NotImplementedException();
        public override ref readonly BlockReference ExtendedFieldInfo => throw new NotImplementedException();
        public override ref readonly BlockReference RelationshipTable => throw new NotImplementedException();

        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
            _stringTableRef = new BlockReference(Structure.StringTableLength != 0, Structure.StringTableLength);
        }
    }
}
