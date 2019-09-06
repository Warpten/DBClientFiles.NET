using DBClientFiles.NET.Parsing.Shared.Segments;

namespace DBClientFiles.NET.Parsing.Shared.Headers
{
    internal interface IHeaderAccessor
    {
        int RecordCount { get; }
        int FieldCount { get; }
        int RecordSize { get; }

        ref readonly SegmentReference OffsetMap { get; }
        ref readonly SegmentReference IndexTable { get; }
        ref readonly SegmentReference Pallet { get; }
        ref readonly SegmentReference Common { get; }
        ref readonly SegmentReference CopyTable { get; }
        ref readonly SegmentReference StringTable { get; }

        ref readonly SegmentReference FieldInfo { get; }
        ref readonly SegmentReference ExtendedFieldInfo { get; }
        ref readonly SegmentReference RelationshipTable { get; }

        int MinIndex { get; }
        int MaxIndex { get; }
        int IndexColumn { get; }
    }
}
