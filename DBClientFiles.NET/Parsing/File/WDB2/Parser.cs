using System.IO;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;

namespace DBClientFiles.NET.Parsing.File.WDB2
{
    internal sealed class Parser<T> : BinaryFileParser<T, Serializer<T>>
    {
        public override int RecordCount => Header.RecordCount;

        private AlignedRecordReader _recordReader;

        public Parser(in StorageOptions options, Stream input) : base(options, input)
        {
        }

        protected override void Dispose(bool disposing)
        {
            _recordReader.Dispose();
            _recordReader = null;

            base.Dispose(disposing);
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

            var tail = Head = new Block
            {
                Identifier = BlockIdentifier.Header,
                Length = 12 * 4,

                Handler = new HeaderHandler(this)
            };

            if (Header.MaxIndex != 0)
            {
                tail = Head.Next = new Block {
                    Identifier = BlockIdentifier.OffsetMap,
                    Length = (4 + 2) * (Header.MinIndex - Header.MaxIndex + 1)
                };
            }

            tail.Next = new Block {
                Identifier = BlockIdentifier.Records,
                Length = Header.RecordCount * Header.RecordSize,

                Next = new Block {
                    Identifier = BlockIdentifier.StringBlock,
                    Length = Header.StringTableLength,

                    Handler = new StringBlockHandler(Options.InternStrings)
                }
            };
        }

        public override void After(ParsingStep step)
        {
            if (step == ParsingStep.Segments)
                _recordReader = new AlignedRecordReader(this, Header.RecordSize);
        }
    }
}
