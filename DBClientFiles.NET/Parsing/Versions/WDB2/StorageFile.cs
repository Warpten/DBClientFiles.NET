using DBClientFiles.NET.Parsing.Enumerators;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions.WDB2.Segments.Handlers;
using DBClientFiles.NET.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Parsing.Shared.Records;

namespace DBClientFiles.NET.Parsing.Versions.WDB2
{
    internal sealed class StorageFile<T> : BinaryStorageFile<T>
    {
        public override int RecordCount => Header.RecordCount;

        private Serializer<T> _serializer;
        private ISequentialRecordReader _recordReader;

        public StorageFile(in StorageOptions options, Stream input) : base(in options, input)
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            _recordReader.Dispose();
        }

        public override void Before(ParsingStep step)
        {
            if (step != ParsingStep.Segments)
                return;

            var headerHandler = new HeaderHandler(this);
            var stringBlockHandler = new StringBlockHandler();

            var tail = Head = new Segment {
                Identifier = SegmentIdentifier.Header,
                Length = Unsafe.SizeOf<Header>(),

                Handler = headerHandler,
            };

            if (headerHandler.MaxIndex != 0)
            {
                // This is not really an offset map but whatever, no handler is attached.
                tail = Head.Next = new Segment {
                    Identifier = SegmentIdentifier.OffsetMap,
                    Length = (4 + 2) * (Header.MaxIndex - Header.MinIndex + 1)
                };
            }

            tail.Next = new Segment {
                Identifier = SegmentIdentifier.Records,
                Length = headerHandler.RecordCount * headerHandler.RecordSize,

                Next = new Segment {
                    Identifier = SegmentIdentifier.StringBlock,
                    Length = headerHandler.StringTable.Length,

                    Handler = stringBlockHandler
                }
            };

            _recordReader = new AlignedSequentialRecordReader(stringBlockHandler);
        }

        public override void After(ParsingStep step) {
            if (step == ParsingStep.Segments)
                _serializer = new Serializer<T>(this);
        }

        public override T ObtainRecord(long offset, long length)
        {
            DataStream.Position = offset;

            return _serializer.Deserialize(DataStream.Limit(length, false), _recordReader);
        }

        internal override void Clone(in T source, out T clonedInstance) => throw new InvalidOperationException();
        internal override int GetRecordKey(in T value) => throw new InvalidOperationException();
        internal override void SetRecordKey(out T value, int recordKey) => throw new InvalidOperationException();

        protected override IEnumerator<T> CreateEnumerator()
        {
            return !Header.OffsetMap.Exists
                ? (IEnumerator<T>) new RecordsEnumerator<StorageFile<T>, T>(this)
                : (IEnumerator<T>) new OffsetMapEnumerator<StorageFile<T>, T>(this);
        }
    }
}
