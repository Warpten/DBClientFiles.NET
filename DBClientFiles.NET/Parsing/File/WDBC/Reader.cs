using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    internal sealed class Reader<T> : BinaryFileReader<T>
    {
        private Header _fileHeader;
        private ISerializer<T> _generator;

        public override IFileHeader Header => _fileHeader;
        public override ISerializer<T> Serializer => _generator;

        public override int Size => _fileHeader.RecordCount;

        public Reader(StorageOptions options, Stream input) : base(options, input, true)
        {
            _fileHeader = new Header();
            _generator = new Serializer<T>(options, Type);
        }

        protected override IRecordReader GetRecordReader()
        {
            return base.GetRecordReader();
        }

        protected override void PrepareBlocks()
        {
            _fileHeader.Read(this);

            Head.Next = new Block
            {
                Identifier = BlockIdentifier.Records,
                Length = _fileHeader.RecordCount * _fileHeader.RecordSize
            };

            Head.Next.Next = new Block {
                Identifier = BlockIdentifier.StringBlock,
                Length = _fileHeader.StringTableLength
            };
        }
    }
}
