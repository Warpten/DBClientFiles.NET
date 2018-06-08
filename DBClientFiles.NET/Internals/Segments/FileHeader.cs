using DBClientFiles.NET.Collections.Generic;

namespace DBClientFiles.NET.Internals.Segments
{
    internal interface IFileHeader : IStorage
    {
        int RecordSize { get; }
        int RecordCount { get; }
        int FieldCount { get; }

        int StringTableLength { get; }

        // For offsetMap
        int MinIndex { get; }
        int MaxIndex { get; }

        int CopyTableLength { get; }
        int IndexColumn { get; }
        
        bool HasIndexTable { get; }
        bool HasOffsetMap { get; }
    }
}
