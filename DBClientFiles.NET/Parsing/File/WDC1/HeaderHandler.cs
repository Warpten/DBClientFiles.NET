using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.File.WDC1
{
    internal sealed class HeaderHandler : AbstractHeaderHandler<Header>
    {
        public override int RecordCount => Structure.RecordCount;
        public override int FieldCount => Structure.FieldCount;
        public override int RecordSize => Structure.RecordSize;

        public override int IndexColumn => Structure.IndexColumn;

        public override bool HasForeignIds { get; } = false;

        private BlockReference _copyTableRef;
        private BlockReference _commonDataRef;
        private BlockReference _palletDataRef;
        private BlockReference _stringTableRef;
        private BlockReference _offsetMapRef;
        private BlockReference _indexTableRef;
        private BlockReference _fieldInfoRef;
        private BlockReference _extendedFieldInfoRef;
        private BlockReference _relationshipTableRef;

        public override ref readonly BlockReference StringTable       => ref _stringTableRef;
        public override ref readonly BlockReference Common            => ref _commonDataRef;
        public override ref readonly BlockReference CopyTable         => ref _copyTableRef;
        public override ref readonly BlockReference Pallet            => ref _palletDataRef;
        public override ref readonly BlockReference OffsetMap         => ref _offsetMapRef;
        public override ref readonly BlockReference IndexTable        => ref _indexTableRef;
        public override ref readonly BlockReference FieldInfo         => ref _fieldInfoRef;
        public override ref readonly BlockReference ExtendedFieldInfo => ref _extendedFieldInfoRef;
        public override ref readonly BlockReference RelationshipTable => ref _relationshipTableRef;

        public override int MaxIndex => Structure.MaxIndex;
        public override int MinIndex => Structure.MinIndex;

        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
            _copyTableRef = new BlockReference(Structure.CopyTableSize != 0, Structure.CopyTableSize);
            _palletDataRef = new BlockReference(Structure.PalletDataSize != 0, Structure.PalletDataSize);

            _stringTableRef = new BlockReference((Structure.Flags & 0x01) == 0, Structure.StringTableLength);
            _offsetMapRef = new BlockReference((Structure.Flags & 0x01) != 0,
                (Structure.MaxIndex - Structure.MinIndex + 1) * (4 + 2),
                Structure.OffsetMapOffset);

            _indexTableRef = new BlockReference(Structure.IdListSize != 0, Structure.IdListSize);

            _fieldInfoRef = new BlockReference(true, Structure.TotalFieldCount * (2 + 2));
            _extendedFieldInfoRef = new BlockReference(true, Structure.FieldStorageInfoSize);

            _relationshipTableRef = new BlockReference(Structure.RelationshipDataSize != 0, Structure.RelationshipDataSize);
        }
    }
}
