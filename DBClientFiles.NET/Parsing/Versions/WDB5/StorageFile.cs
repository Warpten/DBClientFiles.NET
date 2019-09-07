using DBClientFiles.NET.Parsing.Enumerators;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions.WDB5.Binding;
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

            var tail = Head = new Segment {
                Identifier = SegmentIdentifier.FieldInfo,
                Length = Header.FieldCount * (2 + 2),

                Handler = new FieldInfoHandler<MemberMetadata>()
            };

            tail = tail.Next = new Segment {
                Identifier = SegmentIdentifier.Records,
                Length = Header.OffsetMap.Exists
                    ? Header.StringTable.Length - tail.EndOffset
                    : Header.RecordCount * Header.RecordSize
            };

            if (!Header.OffsetMap.Exists)
            {
                tail = tail.Next = new Segment {
                    Identifier = SegmentIdentifier.StringBlock,
                    Length = Header.StringTable.Length,

                    Handler = new StringBlockHandler()
                };
            }
            else
            {
                tail = tail.Next = new Segment {
                    Identifier = SegmentIdentifier.OffsetMap,
                    Length = (4 + 2) * (Header.MaxIndex - Header.MinIndex + 1),

                    Handler = new OffsetMapHandler()
                };
            }

            if (Header.RelationshipTable.Exists)
            {
                tail = tail.Next = new Segment {
                    // Legacy foreign table, apparently used by only WMOMinimapTexture (WDB3/4/5) @Barncastle
                    Identifier = SegmentIdentifier.RelationshipTable,
                    Length = 4 * (Header.MaxIndex - Header.MinIndex + 1)
                };
            }

            if (Header.IndexTable.Exists)
            {
                tail = tail.Next = new Segment {
                    Identifier = SegmentIdentifier.IndexTable,
                    Length = 4 * Header.RecordCount,

                    Handler = new IndexTableHandler()
                };
            }

            if (Header.CopyTable.Exists)
            {
                tail.Next = new Segment {
                    Identifier = SegmentIdentifier.CopyTable,
                    Length = Header.CopyTable.Length,

                    Handler = new CopyTableHandler()
                };
            }
        }

        public override void After(ParsingStep step)
        {
            if (step != ParsingStep.Segments)
                return;

            _serializer = new Serializer<T>(this);
        }

        protected override IRecordEnumerator<T> CreateEnumerator()
        {
            var enumerator = !Header.OffsetMap.Exists
                ? (Enumerator<StorageFile<T>, T>) new RecordsEnumerator<StorageFile<T>, T>(this)
                : (Enumerator<StorageFile<T>, T>) new OffsetMapEnumerator<StorageFile<T>, T>(this);

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
