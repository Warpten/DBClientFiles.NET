using DBClientFiles.NET.Parsing.Versions;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations
{
    internal interface IHeaderHandler
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

    internal abstract class AbstractHeaderHandler<T> : StructuredBlockHandler<T>, IHeaderHandler where T : struct
    {
        public abstract int RecordCount { get; }
        public abstract int FieldCount { get; }
        public abstract int RecordSize { get; }

        public abstract int MinIndex { get; }
        public abstract int MaxIndex { get; }
        public abstract int IndexColumn { get; }

        private bool _readAlready = false;

        public virtual ref readonly SegmentReference StringTable => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference OffsetMap => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference IndexTable => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference Pallet => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference Common => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference CopyTable => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference FieldInfo => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference ExtendedFieldInfo => ref SegmentReference.Missing;
        public virtual ref readonly SegmentReference RelationshipTable => ref SegmentReference.Missing;

        public AbstractHeaderHandler(IBinaryStorageFile source)
        {
            ReadSegment(source, 0, Unsafe.SizeOf<T>());
        }

        public override void ReadSegment(IBinaryStorageFile reader, long startOffset, long length)
        {
            if (_readAlready)
                return;

            base.ReadSegment(reader, startOffset, length);
            _readAlready = true;
        }
    }
}
