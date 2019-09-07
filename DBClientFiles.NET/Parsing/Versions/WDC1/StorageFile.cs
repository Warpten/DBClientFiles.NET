using System.IO;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions.WDC1.Binding;
using DBClientFiles.NET.Parsing.Enumerators;

namespace DBClientFiles.NET.Parsing.Versions.WDC1
{
    internal sealed class StorageFile<T> : BinaryStorageFile<T>
    {
        public override int RecordCount => Header.RecordCount;

        private Serializer<T> _serializer;

        public StorageFile(in StorageOptions options, in Header header, Stream input) : base(in options, new HeaderAccessor(in header), input)
        {
        }

        public override void Before(ParsingStep step)
        {
            if (step != ParsingStep.Segments)
                return;
           
            var fieldInfoHandler = new FieldInfoHandler<MemberMetadata>();
            var tail = Head = new Segment {
                Identifier = SegmentIdentifier.FieldInfo,
                Length = Header.FieldInfo.Length,

                Handler = fieldInfoHandler
            };

            if (Header.OffsetMap.Exists)
            {
                tail = tail.Next = new Segment {
                    Identifier = SegmentIdentifier.OffsetMap,
                    Length = Header.OffsetMap.Length,

                    Handler = new OffsetMapHandler()
                };
            }
            else
            {
                tail = tail.Next = new Segment {
                    Identifier = SegmentIdentifier.Records,
                    Length = Header.RecordSize * Header.RecordCount,
                };

                tail = tail.Next = new Segment {
                    Identifier = SegmentIdentifier.StringBlock,
                    Length = Header.StringTable.Length,

                    Handler = new StringBlockHandler()
                };
            }

            tail = tail.Next = new Segment {
                Identifier = SegmentIdentifier.IndexTable,
                Length = Header.IndexTable.Length,

                Handler = new IndexTableHandler()
            };

            tail = tail.Next = new Segment {
                Identifier = SegmentIdentifier.CopyTable,
                Length = Header.CopyTable.Length,

                Handler = new CopyTableHandler()
            };

            tail = tail.Next = new Segment {
                Identifier = SegmentIdentifier.ExtendedFieldInfo,
                Length = Header.ExtendedFieldInfo.Length,

                Handler = new ExtendedFieldInfoHandler<MemberMetadata>(fieldInfoHandler)
            };

            tail = tail.Next = new Segment {
                Identifier = SegmentIdentifier.PalletTable,
                Length = Header.Pallet.Length
            };

            tail = tail.Next = new Segment {
                Identifier = SegmentIdentifier.CommonDataTable,
                Length = Header.Common.Length,

                // Handler = not the legacy one!
            };

            tail.Next = new Segment
            {
                Identifier = SegmentIdentifier.RelationshipTable,
                Length = Header.RelationshipTable.Length
            };
        }

        public override void After(ParsingStep step)
        {
            if (step == ParsingStep.Segments)
                _serializer = new Serializer<T>(this);
        }

        protected override IRecordEnumerator<T> CreateEnumerator()
        {
#pragma warning disable IDE0067 // Dispose objects before losing scope
            var enumerator = !Header.OffsetMap.Exists
                ? (Enumerator<StorageFile<T>, T>) new RecordsEnumerator<StorageFile<T>, T>(this)
                : (Enumerator<StorageFile<T>, T>) new OffsetMapEnumerator<StorageFile<T>, T>(this);
#pragma warning restore IDE0067 // Dispose objects before losing scope

            return enumerator.WithIndexTable().WithCopyTable();
        }

        public override T ObtainRecord(long offset, long length)
        {
            DataStream.Position = offset;

            using (var recordReader = new UnalignedRecordReader(this, length))
                return _serializer.Deserialize(recordReader);
        }

        internal override void Clone(in T source, out T clonedInstance) => _serializer.Clone(in source, out clonedInstance);
        internal override int GetRecordKey(in T value) => _serializer.GetRecordKey(in value);
        internal override void SetRecordKey(out T value, int recordKey) => _serializer.SetRecordKey(out value, recordKey);
    }
}
