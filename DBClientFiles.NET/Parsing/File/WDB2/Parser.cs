using System.IO;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;

namespace DBClientFiles.NET.Parsing.File.WDB2
{
    internal sealed class Parser<T> : BinaryFileParser<T, Serializer<T>>
    {
        private IFileHeader _fileHeader;
        public override ref readonly IFileHeader Header => ref _fileHeader;

        public override int RecordCount => _fileHeader.RecordCount;

        private AlignedRecordReader _recordReader;

        public Parser(in StorageOptions options, Stream input) : base(options, input)
        {
            _fileHeader = new Header(this);
        }

        protected override void Dispose(bool disposing)
        {
            _recordReader.Dispose();
            _recordReader = null;

            base.Dispose(disposing);
        }

        public override IRecordReader GetRecordReader(int recordSize)
        {
            _recordReader.LoadStream(BaseStream);
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
            };

            if (_fileHeader.MaxIndex != 0)
            {
                tail = Head.Next = new Block {
                    Identifier = BlockIdentifier.OffsetMap,
                    Length = (4 + 2) * (_fileHeader.MinIndex - _fileHeader.MaxIndex + 1)
                };
            }

            tail.Next = new Block {
                Identifier = BlockIdentifier.Records,
                Length = _fileHeader.RecordCount * _fileHeader.RecordSize,

                Next = new Block {
                    Identifier = BlockIdentifier.StringBlock,
                    Length = _fileHeader.StringTableLength,

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
