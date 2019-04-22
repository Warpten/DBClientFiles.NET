using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations
{
    internal interface IHeaderHandler
    {
        int RecordCount { get; }
        int FieldCount { get; }
        int RecordSize { get; }

        ref readonly BlockReference OffsetMap { get; }
        ref readonly BlockReference IndexTable { get; }
        ref readonly BlockReference Pallet { get; }
        ref readonly BlockReference Common { get; }
        ref readonly BlockReference CopyTable { get; }
        ref readonly BlockReference StringTable { get; }

        ref readonly BlockReference FieldInfo { get; }
        ref readonly BlockReference ExtendedFieldInfo { get; }
        ref readonly BlockReference RelationshipTable { get; }

        bool HasForeignIds { get; }

        int MinIndex { get; }
        int MaxIndex { get; }
        int IndexColumn { get; }
    }

    internal abstract class AbstractHeaderHandler<T> : StructuredBlockHandler<T>, IHeaderHandler where T : struct
    {
        public abstract int RecordCount { get; }
        public abstract int FieldCount { get; }
        public abstract int RecordSize { get; }

        public abstract bool HasForeignIds { get; }

        public abstract int MinIndex { get; }
        public abstract int MaxIndex { get; }
        public abstract int IndexColumn { get; }

        private bool _readAlready = false;

        public abstract ref readonly BlockReference StringTable { get; }
        public abstract ref readonly BlockReference OffsetMap { get; }
        public abstract ref readonly BlockReference IndexTable { get; }
        public abstract ref readonly BlockReference Pallet { get; }
        public abstract ref readonly BlockReference Common { get; }
        public abstract ref readonly BlockReference CopyTable { get; }
        public abstract ref readonly BlockReference FieldInfo { get; }
        public abstract ref readonly BlockReference ExtendedFieldInfo { get; }
        public abstract ref readonly BlockReference RelationshipTable { get; }

        public AbstractHeaderHandler(IBinaryStorageFile source)
        {
            ReadBlock(source, 0, Unsafe.SizeOf<T>());
        }

        public override void ReadBlock(IBinaryStorageFile reader, long startOffset, long length)
        {
            if (_readAlready)
                return;

            base.ReadBlock(reader, startOffset, length);
            _readAlready = true;
        }
    }
}
