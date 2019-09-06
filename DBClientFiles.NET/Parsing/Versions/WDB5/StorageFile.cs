using DBClientFiles.NET.Parsing.Enumerators;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions.WDB5.Binding;
using DBClientFiles.NET.Parsing.Versions.WDB5.Segments.Handlers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Parsing.Versions.WDB5
{
    internal sealed class StorageFile<T> : BinaryStorageFile<T>
    {
        public override int RecordCount => Header.RecordCount + Header.CopyTable.Length / (2 * 4);

        private Serializer<T> _serializer;

        public StorageFile(in StorageOptions options, Stream input) : base(options, input)
        {
        }

        public override void Before(ParsingStep step)
        {
            if (step != ParsingStep.Segments)
                return;

            var headerHandler = new HeaderHandler(this);

            Head = new Segment {
                Identifier = SegmentIdentifier.Header,
                Length = Unsafe.SizeOf<Header>(),

                Handler = headerHandler
            };

            var tail = Head.Next = new Segment {
                Identifier = SegmentIdentifier.FieldInfo,
                Length = headerHandler.FieldCount * (2 + 2),

                Handler = new FieldInfoHandler<MemberMetadata>()
            };

            tail = tail.Next = new Segment {
                Identifier = SegmentIdentifier.Records,
                Length = headerHandler.OffsetMap.Exists
                    ? headerHandler.StringTable.Length - tail.EndOffset
                    : headerHandler.RecordCount * headerHandler.RecordSize
            };

            if (!headerHandler.OffsetMap.Exists)
            {
                tail = tail.Next = new Segment {
                    Identifier = SegmentIdentifier.StringBlock,
                    Length = headerHandler.StringTable.Length,

                    Handler = new StringBlockHandler()
                };
            }
            else
            {
                tail = tail.Next = new Segment {
                    Identifier = SegmentIdentifier.OffsetMap,
                    Length = (4 + 2) * (headerHandler.MaxIndex - headerHandler.MinIndex + 1),

                    Handler = new OffsetMapHandler()
                };
            }

            if (headerHandler.RelationshipTable.Exists)
            {
                tail = tail.Next = new Segment {
                    // Legacy foreign table, apparently used by only WMOMinimapTexture (WDB3/4/5) @Barncastle
                    Identifier = SegmentIdentifier.RelationshipTable,
                    Length = 4 * (headerHandler.MaxIndex - headerHandler.MinIndex + 1)
                };
            }

            if (headerHandler.IndexTable.Exists)
            {
                tail = tail.Next = new Segment {
                    Identifier = SegmentIdentifier.IndexTable,
                    Length = 4 * headerHandler.RecordCount,

                    Handler = new IndexTableHandler()
                };
            }

            if (headerHandler.CopyTable.Exists)
            {
                tail.Next = new Segment {
                    Identifier = SegmentIdentifier.CopyTable,
                    Length = headerHandler.CopyTable.Length,

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

        protected override IEnumerator<T> CreateEnumerator()
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
