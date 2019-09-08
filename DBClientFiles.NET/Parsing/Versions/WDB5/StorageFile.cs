using DBClientFiles.NET.Parsing.Enumerators;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions.WDB5.Binding;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DBClientFiles.NET.Parsing.Versions.WDB5
{
    internal sealed class StorageFile<T> : BinaryStorageFile<T>
    {
        public override int RecordCount => Header.RecordCount + Header.CopyTable.Length / (2 * 4);

        private Serializer<T> _serializer;

        public StorageFile(in StorageOptions options, in Header header, Stream input) : base(options, new HeaderAccessor(in header), input)
        {
        }

        public override void Before(ParsingStep step)
        {
            if (step != ParsingStep.Segments)
                return;

            var tail = Head = new Segment(SegmentIdentifier.FieldInfo, Header.FieldCount * (2 + 2), new FieldInfoHandler<MemberMetadata>());

            tail = tail.Next = new Segment(SegmentIdentifier.Records,
                Header.OffsetMap.Exists
                    ? Header.StringTable.Length - tail.EndOffset
                    : Header.RecordCount * Header.RecordSize);

            if (!Header.OffsetMap.Exists)
                tail = tail.Next = new Segment(SegmentIdentifier.StringBlock, Header.StringTable.Length, new StringBlockHandler());
            else
                tail = tail.Next = new Segment(SegmentIdentifier.OffsetMap, (4 + 2) * (Header.MaxIndex - Header.MinIndex + 1), new OffsetMapHandler());

            // Legacy foreign table, apparently used by only WMOMinimapTexture (WDB3/4/5) @Barncastle
            if (Header.RelationshipTable.Exists)
                tail = tail.Next = new Segment(SegmentIdentifier.RelationshipTable, 4 * (Header.MaxIndex - Header.MinIndex + 1));

            if (Header.IndexTable.Exists)
                tail = tail.Next = new Segment(SegmentIdentifier.IndexTable, 4 * Header.RecordCount, new IndexTableHandler());

            if (Header.CopyTable.Exists)
                tail.Next = new Segment(SegmentIdentifier.CopyTable, Header.CopyTable.Length, new CopyTableHandler());
        }

        public override void After(ParsingStep step)
        {
            if (step != ParsingStep.Segments)
                return;

            _serializer = new Serializer<T>(this);
        }

        [SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", Justification = "False positive - object is returned")]
        protected override IRecordEnumerator<T> CreateEnumerator()
        {
            var enumerator = !Header.OffsetMap.Exists
                ? (Enumerator<T>) new RecordsEnumerator<T>(this)
                : (Enumerator<T>) new OffsetMapEnumerator<T>(this);

            return enumerator.WithIndexTable().WithCopyTable();
        }

        internal override int GetRecordKey(in T value) => _serializer.GetRecordKey(in value);
        internal override void SetRecordKey(out T value, int recordKey) => _serializer.SetRecordKey(out value, recordKey);
        internal override void Clone(in T source, out T clonedInstance) => _serializer.Clone(in source, out clonedInstance);

        public override T ObtainRecord(long offset, long length)
        {
            DataStream.Position = offset;

            using (var recordReader = new ByteAlignedRecordReader(this, (int) length))
                return _serializer.Deserialize(recordReader, this);
        }
    }
}
