using System;
using System.Runtime.InteropServices;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Parsing.Versions.WDB5.Segments.Handlers
{
    internal sealed class HeaderHandler : AbstractHeaderHandler<Either<Header, OldHeader>>
    {
        public override int RecordCount { get; }
        public override int FieldCount { get; }
        public override int RecordSize { get; }

        public override int IndexColumn { get; }

        private SegmentReference _stringTableRef;
        private SegmentReference _indexTable;
        private SegmentReference _offsetMap;
        private SegmentReference _fieldInfoRef;
        private SegmentReference _relationshipTableRef;
        private SegmentReference _copyTableRef;

        public override ref readonly SegmentReference StringTable => ref _stringTableRef;
        public override ref readonly SegmentReference IndexTable => ref _indexTable;
        public override ref readonly SegmentReference OffsetMap => ref _offsetMap;
        public override ref readonly SegmentReference FieldInfo => ref _fieldInfoRef;
        public override ref readonly SegmentReference RelationshipTable => ref _relationshipTableRef;
        public override ref readonly SegmentReference CopyTable => ref _copyTableRef;

        // Blocks this file does not have
        public override ref readonly SegmentReference Common => throw new NotImplementedException();
        public override ref readonly SegmentReference Pallet => throw new NotImplementedException();
        public override ref readonly SegmentReference ExtendedFieldInfo => throw new NotImplementedException();

        public override int MaxIndex { get; }
        public override int MinIndex { get; }

        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
            // the old header (before 21737) has Build instead of LayoutHash.
            // Thus the simple solution to check if a file uses the old or the new header
            // is to check for the high bytes of that field - if any bit is set, it's a layout hash. 
            if (IsRight)
            {
                _fieldInfoRef = new SegmentReference(true, Structure.Right.FieldCount * (2 + 2));

                // String table and offset map are mutually exclusive.
                _stringTableRef = new SegmentReference((Structure.Right.Flags & 0x01) == 0,
                    Structure.Right.StringTableLength);

                _offsetMap = new SegmentReference((Structure.Right.Flags & 0x01) != 0,
                    (Structure.Right.MaxIndex - Structure.Right.MinIndex + 1) * (4 + 2),
                    Structure.Right.StringTableLength);
                _relationshipTableRef = new SegmentReference((Structure.Right.Flags & 0x02) != 0,
                    (Structure.Right.MaxIndex - Structure.Right.MinIndex + 1) * 4);
                _indexTable = new SegmentReference((Structure.Right.Flags & 0x04) != 0,
                    Structure.Right.RecordCount * 4);

                _copyTableRef = new SegmentReference(Structure.Right.CopyTableLength != 0,
                    Structure.Right.CopyTableLength / (4 + 4));

                RecordCount = Structure.Right.RecordCount;
                FieldCount = Structure.Right.FieldCount;
                RecordSize = Structure.Right.RecordSize;

                MaxIndex = Structure.Right.MaxIndex;
                MinIndex = Structure.Right.MinIndex;

                IndexColumn = 0;
            }
            else
            {
                _fieldInfoRef = new SegmentReference(true, Structure.Left.FieldCount * (2 + 2));

                // String table and offset map are mutually exclusive.
                _stringTableRef = new SegmentReference((Structure.Left.Flags & 0x01) == 0,
                    Structure.Left.StringTableLength);

                _offsetMap = new SegmentReference((Structure.Left.Flags & 0x01) != 0,
                    (Structure.Left.MaxIndex - Structure.Left.MinIndex + 1) * (4 + 2),
                    Structure.Left.StringTableLength);
                _relationshipTableRef = new SegmentReference((Structure.Left.Flags & 0x02) != 0,
                    (Structure.Left.MaxIndex - Structure.Left.MinIndex + 1) * 4);
                _indexTable = new SegmentReference((Structure.Left.Flags & 0x04) != 0,
                    Structure.Left.RecordCount * 4);

                _copyTableRef = new SegmentReference(Structure.Left.CopyTableLength != 0,
                    Structure.Left.CopyTableLength / (4 + 4));

                RecordCount = Structure.Left.RecordCount;
                FieldCount = Structure.Left.FieldCount;
                RecordSize = Structure.Left.RecordSize;

                MaxIndex = Structure.Left.MaxIndex;
                MinIndex = Structure.Left.MinIndex;

                IndexColumn = Structure.Left.IndexColumn;
            }
        }

        public bool IsRight => (Structure.Right.Build & 0xFFFF0000) == 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Header
    {
        public readonly Signatures Signature;
        public readonly int RecordCount;
        public readonly int FieldCount;
        public readonly int RecordSize;
        public readonly int StringTableLength;
        public readonly uint TableHash;
        public readonly uint LayoutHash;
        public readonly int MinIndex;
        public readonly int MaxIndex;
        public readonly int Locale;
        public readonly int CopyTableLength;
        public readonly ushort Flags;
        public readonly short IndexColumn;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct OldHeader
    {
        public readonly Signatures Signature;
        public readonly int RecordCount;
        public readonly int FieldCount;
        public readonly int RecordSize;
        public readonly int StringTableLength;
        public readonly uint Build;
        public readonly uint LayoutHash;
        public readonly int MinIndex;
        public readonly int MaxIndex;
        public readonly int Locale;
        public readonly int CopyTableLength;
        public readonly uint Flags;
    }
}
