using System.IO;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;

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

        protected override IRecordReader GetRecordReader()
        {
            _recordReader.LoadStream(BaseStream);
            return _recordReader;
        }

        protected override void Prepare()
        {
            Head.Next = new Block {
                // Identifier is not really relevant, since we won't parse it anyways.
                Identifier = BlockIdentifier.OffsetMap,
                Length = (Header.MaxIndex - Header.MinIndex + 1) * (4 + 2)
            };

            Head.Next.Next = new Block {
                Identifier = BlockIdentifier.Records,
                Length = _fileHeader.RecordCount * _fileHeader.RecordSize
            };

            Head.Next.Next.Next = new Block {
                Identifier = BlockIdentifier.StringBlock,
                Length = _fileHeader.StringTableLength
            };

            _recordReader = new AlignedRecordReader(this, Header.RecordSize);
        }

        public override BaseMemberMetadata GetFileMemberMetadata(int index)
        {
            throw new System.NotImplementedException();
        }
    }
}
