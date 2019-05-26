using System;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.File.WDB5
{
    internal sealed class HeaderHandler : AbstractHeaderHandler<Header>
    {
        public override int RecordCount => Structure.RecordCount;
        public override int FieldCount => Structure.FieldCount;
        public override int RecordSize => Structure.RecordSize;

        public override int IndexColumn { get; } = 0;

        private BlockReference _stringTableRef;
        private BlockReference _indexTable;
        private BlockReference _offsetMap;
        private BlockReference _fieldInfoRef;
        private BlockReference _relationshipTableRef;
        private BlockReference _copyTableRef;

        public override ref readonly BlockReference StringTable => ref _stringTableRef;
        public override ref readonly BlockReference IndexTable => ref _indexTable;
        public override ref readonly BlockReference OffsetMap => ref _offsetMap;
        public override ref readonly BlockReference FieldInfo => ref _fieldInfoRef;
        public override ref readonly BlockReference RelationshipTable => ref _relationshipTableRef;
        public override ref readonly BlockReference CopyTable => ref _copyTableRef;

        // Blocks this file does not have
        public override ref readonly BlockReference Common => throw new NotImplementedException();
        public override ref readonly BlockReference Pallet => throw new NotImplementedException();
        public override ref readonly BlockReference ExtendedFieldInfo => throw new NotImplementedException();

        public override int MaxIndex => Structure.MaxIndex;
        public override int MinIndex => Structure.MinIndex;

        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
            _fieldInfoRef = new BlockReference(true, Structure.FieldCount * (2 + 2));

            // String table and offset map are mutually exclusive.
            _stringTableRef = new BlockReference((Structure.Flags & 0x01) == 0, Structure.StringTableLength);

            _offsetMap = new BlockReference((Structure.Flags & 0x01) != 0, (Structure.MaxIndex - Structure.MinIndex + 1) * (4 + 2), Structure.StringTableLength);
            _relationshipTableRef = new BlockReference((Structure.Flags & 0x02) != 0, (Structure.MaxIndex - Structure.MinIndex + 1) * 4);
            _indexTable = new BlockReference((Structure.Flags & 0x04) != 0, Structure.RecordCount * 4);

            _copyTableRef = new BlockReference(Structure.CopyTableLength != 0, Structure.CopyTableLength / (4 + 4));
        }
    }
}
