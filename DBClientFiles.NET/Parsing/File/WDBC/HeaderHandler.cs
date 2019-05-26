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

        // Blocks that don't exist
        public override ref readonly BlockReference OffsetMap         => throw new UnknownBlockException(BlockIdentifier.OffsetMap, Signatures.WDBC);
        public override ref readonly BlockReference IndexTable        => throw new UnknownBlockException(BlockIdentifier.IndexTable, Signatures.WDBC);
        public override ref readonly BlockReference Pallet            => throw new UnknownBlockException(BlockIdentifier.PalletTable, Signatures.WDBC);
        public override ref readonly BlockReference CopyTable         => throw new UnknownBlockException(BlockIdentifier.CopyTable, Signatures.WDBC);
        public override ref readonly BlockReference Common            => throw new UnknownBlockException(BlockIdentifier.CommonDataTable, Signatures.WDBC);
        public override ref readonly BlockReference FieldInfo         => throw new UnknownBlockException(BlockIdentifier.FieldInfo, Signatures.WDBC);
        public override ref readonly BlockReference ExtendedFieldInfo => throw new UnknownBlockException(BlockIdentifier.ExtendedFieldInfo, Signatures.WDBC);
        public override ref readonly BlockReference RelationshipTable => throw new UnknownBlockException(BlockIdentifier.RelationshipTable, Signatures.WDBC);

        // Properties that don't exist
        public override int MaxIndex => throw new InvalidOperationException();
        public override int MinIndex => throw new InvalidOperationException();

        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
            _stringTableRef = new BlockReference(Structure.StringTableLength != 0, Structure.StringTableLength);
        }
    }
}
