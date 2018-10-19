using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Types;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    internal sealed class Writer<T> : BinaryFileWriter<T>
    {
        private Header _fileHeader;
        private ISerializer<T> _generator;

        public override IFileHeader Header => _fileHeader;
        public override ISerializer<T> Serializer => _generator;

        public Writer(StorageOptions options, Stream outputStream, bool keepOpen) : base(options, outputStream, keepOpen)
        {
            _fileHeader = new Header();
            _generator = new Serializer<T>(options, TypeInfo.Create<T>());
        }

        protected override void PrepareBlocks()
        {
            Head.Next = new Block
            {
                Identifier = BlockIdentifier.Records,
                Length = _fileHeader.RecordCount * _fileHeader.RecordSize
            };

            Head.Next.Next = new Block
            {
                Identifier = BlockIdentifier.StringBlock,
                Length = _fileHeader.StringTableLength
            };
        }
    }
}
