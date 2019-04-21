using System;
using System.IO;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;

namespace DBClientFiles.NET.Parsing.File.WDC1
{
    internal sealed class Parser<T> : BinaryFileParser<T, Serializer<T>>
    {
        public override int RecordCount => Header.RecordCount;

        private AlignedRecordReader _recordReader;

        public Parser(in StorageOptions options, Stream input) : base(in options, input)
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
