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

        public virtual ref readonly BlockReference StringTable => ref BlockReference.Missing;
        public virtual ref readonly BlockReference OffsetMap => ref BlockReference.Missing;
        public virtual ref readonly BlockReference IndexTable => ref BlockReference.Missing;
        public virtual ref readonly BlockReference Pallet => ref BlockReference.Missing;
        public virtual ref readonly BlockReference Common => ref BlockReference.Missing;
        public virtual ref readonly BlockReference CopyTable => ref BlockReference.Missing;
        public virtual ref readonly BlockReference FieldInfo => ref BlockReference.Missing;
        public virtual ref readonly BlockReference ExtendedFieldInfo => ref BlockReference.Missing;
        public virtual ref readonly BlockReference RelationshipTable => ref BlockReference.Missing;

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
