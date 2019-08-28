using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.Versions.WDB2.Segments.Handlers
{
    internal sealed class HeaderHandler : AbstractHeaderHandler<Header>
    {
        public override int RecordCount => Structure.RecordCount;
        public override int FieldCount => Structure.FieldCount;
        public override int RecordSize => Structure.RecordSize;
        public override int MaxIndex => Structure.MaxIndex;
        public override int MinIndex => Structure.MinIndex;

        public override int IndexColumn { get; } = 0;

        private SegmentReference _stringTableRef;
        public override ref readonly SegmentReference StringTable => ref _stringTableRef;

        public HeaderHandler(IBinaryStorageFile source) : base(source)
        {
            _stringTableRef = new SegmentReference(Structure.StringTableLength != 0, Structure.StringTableLength);
        }
    }

    /// <summary>
    /// Representation of a WDB2 header.
    ///
    /// See <a href="http://www.wowdev.wiki/DBC">the wiki</a>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Header
    {
        public readonly Signatures Signature;
        public readonly int RecordCount;
        public readonly int FieldCount;
        public readonly int RecordSize;
        public readonly int StringTableLength;
        public readonly uint TableHash;
        public readonly uint Build;
        public readonly uint TimestampLastWritten;
        public readonly int MinIndex;
        public readonly int MaxIndex;
        public readonly int Locale;
        public readonly int CopyTableLength;
    }
}
