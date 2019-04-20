using System;
using System.IO;
using System.Runtime.Serialization;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.File.WDC1
{
    internal sealed class Parser<T> : BinaryFileParser<T, Serializer<T>>
    {
        private IFileHeader _fileHeader;
        public override ref readonly IFileHeader Header => ref _fileHeader;

        public override int RecordCount => _fileHeader.RecordCount;

        private AlignedRecordReader _recordReader;

        public Parser(in StorageOptions options, Stream input) : base(in options, input)
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
            
            Head = new Block {
                Identifier = BlockIdentifier.Header,
                Length = 11 * 4 + 2 * 2 + 9 * 4
            };

            throw new NotImplementedException();
        }

        public override void After(ParsingStep step)
        {
            if (step == ParsingStep.Segments)
                _recordReader = new AlignedRecordReader(this, Header.RecordSize);
        }
    }
}
