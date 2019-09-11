using DBClientFiles.NET.Parsing.Enumerators;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Utils.Extensions;
using System;
using System.IO;
using DBClientFiles.NET.Parsing.Shared.Records;

namespace DBClientFiles.NET.Parsing.Versions.WDB2
{
    internal sealed class StorageFile<T> : BinaryStorageFile<T>
    {
        public override int RecordCount => Header.RecordCount;

        // Reuse the WDBC deserializer because it hasn't changed.
        private WDBC.RuntimeDeserializer<T> _serializer;
        private AlignedSequentialRecordReader _recordReader;

        public StorageFile(in StorageOptions options, in Header header, Stream input) : base(in options, new HeaderAccessor(in header), input)
        {
            _serializer = new WDBC.RuntimeDeserializer<T>(Type, Options.TokenType);
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

            var stringBlockHandler = new StringBlockHandler();

            if (Header.MaxIndex != 0)
            {
                Head = new Segment(SegmentIdentifier.Ignored, (4 + 2) * (Header.MaxIndex - Header.MinIndex + 1)) {
                    Next = new Segment(SegmentIdentifier.Records, Header.RecordCount * Header.RecordSize) {
                        Next = new Segment(SegmentIdentifier.StringBlock, Header.StringTable.Length, stringBlockHandler)
                    }
                };
            }
            else
            {
                Head = new Segment(SegmentIdentifier.Records, Header.RecordCount * Header.RecordSize) {
                    Next = new Segment(SegmentIdentifier.StringBlock, Header.StringTable.Length, stringBlockHandler)
                };
            }

            _recordReader = new AlignedSequentialRecordReader(stringBlockHandler);
        }

        public override void After(ParsingStep step) { }

        public override T ObtainRecord(long offset, long length)
        {
            DataStream.Position = offset;

            using (var recordStream = DataStream.Limit(length, false))
                return _serializer.Deserialize(recordStream, in _recordReader);
        }

        internal override void Clone(in T source, out T clonedInstance) => throw new InvalidOperationException();
        internal override int GetRecordKey(in T value) => throw new InvalidOperationException();
        internal override void SetRecordKey(out T value, int recordKey) => throw new InvalidOperationException();

        protected override IRecordEnumerator<T> CreateEnumerator()
        {
            return !Header.OffsetMap.Exists
                ? (IRecordEnumerator<T>) new RecordsEnumerator<T>(this)
                : (IRecordEnumerator<T>) new OffsetMapEnumerator<T>(this);
        }
    }
}
