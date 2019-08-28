using DBClientFiles.NET.Parsing.Enumerators;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions.WDB2.Segments.Handlers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Parsing.Versions.WDB2
{
    internal sealed class Parser<T> : BinaryFileParser<T, Serializer<T>>
    {
        public override int RecordCount => Header.RecordCount;

        private AlignedRecordReader _recordReader;

        public Parser(in StorageOptions options, Stream input) : base(options, input)
        {
        }

        public override void Dispose()
        {
            _recordReader.Dispose();
            _recordReader = null;

            base.Dispose();
        }

        public override IRecordReader GetRecordReader(int recordSize)
        {
            _recordReader.LoadStream(BaseStream, recordSize);
            return _recordReader;
        }

        public override void Before(ParsingStep step)
        {
            if (step != ParsingStep.Segments)
                return;

            var headerHandler = new HeaderHandler(this);

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

                    Handler = new StringBlockHandler(Options.InternStrings)
                }
            };
        }

        public override void After(ParsingStep step)
        {
            if (step == ParsingStep.Segments)
                _recordReader = new AlignedRecordReader(this, Header.RecordSize);
        }

        protected override IEnumerator<T> CreateEnumerator()
        {
            return !Header.OffsetMap.Exists
                ? (IEnumerator<T>) new RecordsEnumerator<Parser<T>, T, Serializer<T>>(this)
                : (IEnumerator<T>) new OffsetMapEnumerator<Parser<T>, T, Serializer<T>>(this);
        }
    }
}
