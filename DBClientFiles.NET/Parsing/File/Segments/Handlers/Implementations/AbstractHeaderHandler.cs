using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations
{
    internal interface IHeaderHandler
    {
        int RecordCount { get; }
        int FieldCount { get; }
        int RecordSize { get; }
        int StringTableLength { get; }

        bool HasIndexTable { get; }
        bool HasOffsetMap { get; }
        bool HasForeignIds { get; }

        int MinIndex { get; }
        int MaxIndex { get; }
        int CopyTableLength { get; }
        int IndexColumn { get; }
    }

    internal abstract class AbstractHeaderHandler<T> : StructuredBlockHandler<T>, IHeaderHandler where T : struct
    {
        public abstract int RecordCount { get; }
        public abstract int FieldCount { get; }
        public abstract int RecordSize { get; }
        public abstract int StringTableLength { get; }

        public abstract bool HasIndexTable { get; }
        public abstract bool HasOffsetMap { get; }
        public abstract bool HasForeignIds { get; }

        public abstract int MinIndex { get; }
        public abstract int MaxIndex { get; }
        public abstract int CopyTableLength { get; }
        public abstract int IndexColumn { get; }

        private bool _readAlready = false;

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
