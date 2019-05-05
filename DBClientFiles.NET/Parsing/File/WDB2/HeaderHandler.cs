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

        public override bool HasForeignIds => throw new InvalidOperationException();

        private BlockReference _stringTableRef;

        public override ref readonly BlockReference StringTable => ref _stringTableRef;

        // Blocks that don't exist
        public override ref readonly BlockReference OffsetMap         => throw new UnknownBlockException(BlockIdentifier.OffsetMap, Signatures.WDB2);
        public override ref readonly BlockReference IndexTable        => throw new UnknownBlockException(BlockIdentifier.IndexTable, Signatures.WDB2);
        public override ref readonly BlockReference Pallet            => throw new UnknownBlockException(BlockIdentifier.PalletTable, Signatures.WDB2);
        public override ref readonly BlockReference Common            => throw new UnknownBlockException(BlockIdentifier.CommonDataTable, Signatures.WDB2);
        public override ref readonly BlockReference CopyTable         => throw new UnknownBlockException(BlockIdentifier.CopyTable, Signatures.WDB2);
        public override ref readonly BlockReference FieldInfo         => throw new UnknownBlockException(BlockIdentifier.FieldInfo, Signatures.WDB2);
        public override ref readonly BlockReference ExtendedFieldInfo => throw new UnknownBlockException(BlockIdentifier.ExtendedFieldInfo, Signatures.WDB2);
        public override ref readonly BlockReference RelationshipTable => throw new UnknownBlockException(BlockIdentifier.RelationshipTable, Signatures.WDB2);

        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
            _stringTableRef = new BlockReference(Structure.StringTableLength != 0, Structure.StringTableLength);
        }
    }
}
