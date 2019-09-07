using DBClientFiles.NET.Parsing.Shared.Segments;

namespace DBClientFiles.NET.Parsing.Shared.Headers
{

    internal abstract class AbstractHeaderAccessor<T> : IHeaderAccessor where T : struct, IHeader
    {
        public abstract int RecordCount { get; }
        public abstract int FieldCount { get; }
        public abstract int RecordSize { get; }

        public abstract int MinIndex { get; }
        public abstract int MaxIndex { get; }
        public abstract int IndexColumn { get; }

        public virtual ref readonly SegmentReference StringTable => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference OffsetMap => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference IndexTable => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference Pallet => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference Common => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference CopyTable => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference FieldInfo => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference ExtendedFieldInfo => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference RelationshipTable => ref SegmentReference.Missing;

        // TODO: Remove this (not really needed tbf)
        protected T Header { get; }

        protected AbstractHeaderAccessor(in T instance)
        {
            Header = instance;
        }
    }
}
