using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    /// <summary>
    /// Handles WDBC parsing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
                Length = Unsafe.SizeOf<Header>(),

                Handler = new HeaderHandler(this)
            };

            var headerInfo = (IHeaderHandler) Head.Handler;

            Head.Next = new Block {
                Identifier = BlockIdentifier.Records,
                Length = headerInfo.RecordCount * headerInfo.RecordSize,

                Next = new Block {
                    Identifier = BlockIdentifier.StringBlock,
                    Length = headerInfo.StringTable.Length,

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
