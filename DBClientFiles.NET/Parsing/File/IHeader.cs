using System.IO;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Parsing.File
{
    public readonly struct Header
    {
        public readonly Signatures Signature;
        public readonly uint TableHash;
        public readonly uint LayoutHash;

        internal Header(IFileHeader source)
        {
            Signature = source.Signature;
            if (source.Signature != Signatures.WDBC)
            {
                TableHash = source.TableHash;
                LayoutHash = source.LayoutHash;
            }
            else
            {
                TableHash = LayoutHash = 0;
            }
        }

        internal Header(Header other)
        {
            Signature = other.Signature;
            TableHash = other.TableHash;
            LayoutHash = other.LayoutHash;
        }
    }

    public interface IFileHeader
    {
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
