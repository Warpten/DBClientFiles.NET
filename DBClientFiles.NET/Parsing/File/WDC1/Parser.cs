using System;
using System.IO;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.File.WDC1
{
    internal sealed class Parser<T> : BinaryFileParser<T, Serializer<T>>
    {
        public override int RecordCount => Header.RecordCount;

        private UnalignedRecordReader _recordReader;

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

            var tail = Head.Next = new Block
            {
                Identifier = BlockIdentifier.FieldInfo,
                Length = Header.FieldInfo.Length,

                Handler = new FieldInfoHandler<MemberMetadata>()
            };

            if (Header.OffsetMap.Exists)
            {
                tail = tail.Next = new Block {
                    Identifier = BlockIdentifier.Records,
                    Length = Header.OffsetMap.Offset.Value - tail.EndOffset,

                };

                tail = tail.Next = new Block
                {
                    Identifier = BlockIdentifier.OffsetMap,
                    Length = Header.OffsetMap.Length,

                    Handler = new OffsetMapHandler()
                };
            }
            else
            {
                tail = tail.Next = new Block
                {
                    Identifier = BlockIdentifier.Records,
                    Length = Header.RecordSize - Header.RecordCount,

                };

                tail = tail.Next = new Block
                {
                    Identifier = BlockIdentifier.StringBlock,
                    Length = Header.StringTable.Length,

                    Handler = new StringBlockHandler(Options.InternStrings)
                };
            }

            tail = tail.Next = new Block
            {
                Identifier = BlockIdentifier.IndexTable,
                Length = Header.IndexTable.Length,

                Handler = new IndexTableHandler()
            };

            tail = tail.Next = new Block
            {
                Identifier = BlockIdentifier.CopyTable,
                Length = Header.CopyTable.Length,

                Handler = new CopyTableHandler()
            };

            tail = tail.Next = new Block
            {
                Identifier = BlockIdentifier.FieldPackInfo,
                Length = Header.ExtendedFieldInfo.Length
            };

            tail = tail.Next = new Block
            {
                Identifier = BlockIdentifier.PalletTable,
                Length = Header.Pallet.Length
            };

            tail = tail.Next = new Block
            {
                Identifier = BlockIdentifier.CommonDataTable,
                Length = Header.Common.Length,

                // Handler = not the legacy one!
            };

            tail = tail.Next = new Block
            {
                Identifier = BlockIdentifier.RelationshipTable,
                Length = Header.RelationshipTable.Length
            };

            throw new NotImplementedException();
        }

        public override void After(ParsingStep step)
        {
            if (step == ParsingStep.Segments)
                _recordReader = new UnalignedRecordReader(this, Header.RecordSize);
        }
    }
}
