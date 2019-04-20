using System.IO;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.File.WDB5
{
    internal sealed class Parser<T> : BinaryFileParser<T, Serializer<T>>
    {
        private IFileHeader _fileHeader;
        public override ref readonly IFileHeader Header => ref _fileHeader;

        public override int RecordCount => _fileHeader.RecordCount + _fileHeader.CopyTableLength / 2;

        private BytePackedRecordReader _recordReader;

        public Parser(in StorageOptions options, Stream input) : base(options, input)
        {
            _fileHeader = new Header(this);
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
            
            Head = new Block
            {
                Identifier = BlockIdentifier.Header,
                Length = 11 * 4 + 2 * 2,

                Handler = new FieldInfoHandler<MemberMetadata>()
            };

            var tail = Head.Next = new Block
            {
                Identifier = BlockIdentifier.FieldInfo,
                Length = Header.FieldCount * (2 + 2)
            };

            tail = tail.Next = new Block
            {
                Identifier = BlockIdentifier.Records,
                Length = Header.HasOffsetMap
                    ? Header.StringTableLength - tail.EndOffset
                    : Header.RecordCount * Header.RecordSize
            };

            if (!Header.HasOffsetMap)
            {
                tail = tail.Next = new Block {
                    Identifier = BlockIdentifier.StringBlock,
                    Length = Header.StringTableLength,

                    Handler = new StringBlockHandler(Options.InternStrings)
                };
            }
            else
            {
                tail = tail.Next = new Block {
                    Identifier = BlockIdentifier.OffsetMap,
                    Length = (4 + 2) * (Header.MaxIndex - Header.MinIndex + 1),

                    Handler = new OffsetMapHandler()
                };
            }

            if (Header.HasForeignIds)
            {
                tail = tail.Next = new Block {
                    Identifier = BlockIdentifier.RelationShipTable,
                    Length = 4 * (Header.MaxIndex - Header.MinIndex + 1)
                };
            }

            if (Header.HasIndexTable)
            {
                tail = tail.Next = new Block {
                    Identifier = BlockIdentifier.IndexTable,
                    Length = 4 * Header.RecordCount,

                    Handler = new IndexTableHandler()
                };
            }

            if (Header.CopyTableLength > 0)
            {
                tail = tail.Next = new Block {
                    Identifier = BlockIdentifier.CopyTable,
                    Length = Header.CopyTableLength,

                    Handler = new CopyTableHandler()
                };
            }
        }

        public override void After(ParsingStep step)
        {
            if (step != ParsingStep.Segments)
                return;
            
            _recordReader = Header.HasOffsetMap 
                ? new BytePackedRecordReader(this, Header.RecordSize)
                : new BytePackedRecordReader(this, ((OffsetMapHandler) FindBlock(BlockIdentifier.OffsetMap).Handler).GetLargestRecordSize());
        }
    }
}
