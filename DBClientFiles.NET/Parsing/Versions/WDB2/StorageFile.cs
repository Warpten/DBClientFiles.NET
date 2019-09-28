using DBClientFiles.NET.Parsing.Enumerators;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Utils.Extensions;
using System;
using System.IO;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Runtime.Serialization;

namespace DBClientFiles.NET.Parsing.Versions.WDB2
{
    internal sealed class StorageFile<T> : BinaryStorageFile<T>
    {
        // Reuse the WDBC deserializer because it hasn't changed.
        private readonly WDBC.RuntimeDeserializer<T> _serializer;
        private AlignedSequentialRecordReader _recordReader;

        public StorageFile(in StorageOptions options, in Header header, Stream input) : base(in options, new HeaderAccessor(in header), input)
        {
            _serializer = new WDBC.RuntimeDeserializer<T>(Type, Options.TokenType);
        }

        public override void Before(ParsingStep step)
        {
            if (step != ParsingStep.Segments)
                return;

            var stringBlock = new StringBlockHandler();

            if (Header.MaxIndex != 0)
            {
                Head = new Segment(SegmentIdentifier.Ignored, (4 + 2) * (Header.MaxIndex - Header.MinIndex + 1)) {
                    Next = new Segment(SegmentIdentifier.Records, Header.RecordCount * Header.RecordSize) {
                        Next = new Segment(SegmentIdentifier.StringBlock, Header.StringTable.Length, stringBlock)
                    }
                };
            }
            else
            {
                Head = new Segment(SegmentIdentifier.Records, Header.RecordCount * Header.RecordSize) {
                    Next = new Segment(SegmentIdentifier.StringBlock, Header.StringTable.Length, stringBlock)
                };
            }

            _recordReader = new AlignedSequentialRecordReader(stringBlock);
        }

        public override void After(ParsingStep step) { }

        public override T ObtainRecord(long offset, long length)
        {
            DataStream.Position = offset;

            _serializer.Method(DataStream, _recordReader, out var instance);
            return instance;
        }

        protected override IRecordEnumerator<T> CreateEnumerator()
        {
            return !Header.OffsetMap.Exists
                ? (IRecordEnumerator<T>) new RecordsEnumerator<T>(this)
                : (IRecordEnumerator<T>) new OffsetMapEnumerator<T>(this);
        }
    }
}
