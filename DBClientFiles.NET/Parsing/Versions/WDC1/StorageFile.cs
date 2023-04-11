using System.IO;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions.WDC1.Binding;
using DBClientFiles.NET.Parsing.Enumerators;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging.Abstractions;

namespace DBClientFiles.NET.Parsing.Versions.WDC1
{
    internal sealed class StorageFile<T> : BinaryStorageFile<T>
    {
        private Serializer<T> _serializer;
        private StringBlockHandler _stringBlock = null;
        private PalletBlockHandler _palletBlock = null;

        public StorageFile(in StorageOptions options, in Header header, Stream input) : base(in options, new HeaderAccessor(in header), input)
        {
        }

        public override void Before(ParsingStep step)
        {
            if (step != ParsingStep.Segments)
                return;
           
            var fieldInfoHandler = new FieldInfoHandler<MemberMetadata>();
            var tail = Head = new Segment(SegmentIdentifier.FieldInfo, Header.FieldInfo.Length, fieldInfoHandler);

            if (Header.OffsetMap.Exists)
                tail = tail.Next = new Segment(SegmentIdentifier.OffsetMap, Header.OffsetMap.Length, new OffsetMapHandler());
            else
            {
                tail = tail.Next = new Segment(SegmentIdentifier.Records, Header.RecordSize * Header.RecordCount);
                tail = tail.Next = new Segment(SegmentIdentifier.StringBlock, Header.StringTable.Length, _stringBlock = new StringBlockHandler());
            }

            tail = tail.Next = new Segment(SegmentIdentifier.IndexTable, Header.IndexTable.Length, new IndexTableHandler());
            tail = tail.Next = new Segment(SegmentIdentifier.CopyTable, Header.CopyTable.Length, new CopyTableHandler());
            tail = tail.Next = new Segment(SegmentIdentifier.ExtendedFieldInfo, Header.ExtendedFieldInfo.Length, new ExtendedFieldInfoHandler<MemberMetadata>(fieldInfoHandler));
            tail = tail.Next = new Segment(SegmentIdentifier.PalletTable, Header.Pallet.Length, _palletBlock = new PalletBlockHandler());
            tail = tail.Next = new Segment(SegmentIdentifier.CommonDataTable, Header.Common.Length);
            tail.Next = new Segment(SegmentIdentifier.RelationshipTable, Header.RelationshipTable.Length);
        }

        public override void After(ParsingStep step)
        {
            if (step == ParsingStep.Segments)
                _serializer = new Serializer<T>(this);
        }

        [SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", Justification = "False positive")]
        protected override IRecordEnumerator<T> CreateEnumerator()
        {
            var enumerator = !Header.OffsetMap.Exists
                ? (Enumerator<T>) new RecordsEnumerator<T>(this)
                : (Enumerator<T>) new OffsetMapEnumerator<T>(this);

            return enumerator.WithIndexTable().WithCopyTable();
        }

        public override T ObtainRecord(long offset, long length)
        {
            DataStream.Position = offset;

            using (var recordReader = new UnalignedRecordReader(this, length, _stringBlock, _palletBlock))
                return _serializer.Deserialize(recordReader);
        }
    }
}
