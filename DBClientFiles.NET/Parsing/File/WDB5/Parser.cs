using System.IO;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;

namespace DBClientFiles.NET.Parsing.File.WDB5
{
    internal sealed class Parser<T> : BinaryFileParser<T, Serializer<T>>
    {
        public override int RecordCount => Header.RecordCount + Header.CopyTable.Length / (2 * 4);

        private ByteAlignedRecordReader _recordReader;

        public Parser(in StorageOptions options, Stream input) : base(options, input)
        {
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

            var tail = Head.Next = new Block {
                Identifier = BlockIdentifier.FieldInfo,
                Length = Header.FieldCount * (2 + 2),

                Handler = new FieldInfoHandler<MemberMetadata>()
            };

            tail = tail.Next = new Block
            {
                Identifier = BlockIdentifier.Records,
                Length = Header.OffsetMap.Exists
                    ? Header.StringTable.Length - tail.EndOffset
                    : Header.RecordCount * Header.RecordSize
            };

            if (!Header.OffsetMap.Exists)
            {
                tail = tail.Next = new Block {
                    Identifier = BlockIdentifier.StringBlock,
                    Length = Header.StringTable.Length,

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

            if (Header.RelationshipTable.Exists)
            {
                tail = tail.Next = new Block {
                    // Legacy foreign table, apparently used by only WMOMinimapTexture (WDC3/4/5) @Barncastle
                    Identifier = BlockIdentifier.RelationshipTable,
                    Length = 4 * (Header.MaxIndex - Header.MinIndex + 1)
                };
            }

            if (Header.IndexTable.Exists)
            {
                tail = tail.Next = new Block {
                    Identifier = BlockIdentifier.IndexTable,
                    Length = 4 * Header.RecordCount,

                    Handler = new IndexTableHandler()
                };
            }

            if (Header.CopyTable.Exists)
            {
                tail.Next = new Block {
                    Identifier = BlockIdentifier.CopyTable,
                    Length = Header.CopyTable.Length,

                    Handler = new CopyTableHandler()
                };
            }
        }

        public override void After(ParsingStep step)
        {
            if (step != ParsingStep.Segments)
                return;
            
            // Pocket sized optimization to allocate a buffer large enough to contain every record without the need for reallocations
            _recordReader = Header.OffsetMap.Exists 
                ? new ByteAlignedRecordReader(this, Header.RecordSize)
                : new ByteAlignedRecordReader(this, ((OffsetMapHandler) FindBlock(BlockIdentifier.OffsetMap).Handler).GetLargestRecordSize());
        }
    }
}
