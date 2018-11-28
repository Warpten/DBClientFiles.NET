using System.IO;

namespace DBClientFiles.NET.Parsing.File
{
    public interface IFileHeader
    {
        int Size { get; }
        Signatures Signature { get; }

        uint TableHash { get; }
        uint LayoutHash { get; }

        int RecordSize { get; }
        int RecordCount { get; }
        int FieldCount { get; }

        int StringTableLength { get; }

        // For offsetMap
        int MinIndex { get; }
        int MaxIndex { get; }

        int CopyTableLength { get; }
        short IndexColumn { get; }

        bool HasIndexTable { get; }
        bool HasForeignIds { get; }
        bool HasOffsetMap { get; }
    }
}
