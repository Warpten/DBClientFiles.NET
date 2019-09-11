using DBClientFiles.NET.Parsing.Enumerators;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Utils.Extensions;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Parsing.Versions.WDBC
{
    /// <summary>
    /// Handles WDBC parsing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class StorageFile<T> : BinaryStorageFile<T>
    {
        public override int RecordCount => Header.RecordCount;

        private RuntimeDeserializer<T> _serializer;
        private AlignedSequentialRecordReader _recordReader;

        public StorageFile(in StorageOptions options, in Header header, Stream input) : base(in options, new HeaderAccessor(in header), input)
        {
            _serializer = new RuntimeDeserializer<T>(Type, Options.TokenType);
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

            Head = new Segment(SegmentIdentifier.Records, Header.RecordCount * Header.RecordSize) {
                Next = new Segment(SegmentIdentifier.StringBlock, Header.StringTable.Length, stringBlockHandler)
            };

            _recordReader = new AlignedSequentialRecordReader(stringBlockHandler);
        }

        public override void After(ParsingStep step)
        {
        }

        public override T ObtainRecord(long offset, long length)
        {
            DataStream.Position = offset;

            var instance = default(T);
            using (var recordStream = DataStream.Limit(length, false))
                _serializer.Method(DataStream, in _recordReader, out instance);

            return instance;
        }

        internal override int GetRecordKey(in T value) => throw new InvalidOperationException();
    
        internal override void SetRecordKey(out T value, int recordKey) => throw new InvalidOperationException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Clone(in T source, out T clonedInstance) => throw new InvalidOperationException();

        protected override IRecordEnumerator<T> CreateEnumerator() => new RecordsEnumerator<T>(this);
    }
}
