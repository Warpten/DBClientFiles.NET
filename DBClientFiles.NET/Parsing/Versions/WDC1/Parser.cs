using System.IO;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions.WDC1.Segments.Handlers;
using DBClientFiles.NET.Parsing.Versions.WDC1.Binding;

namespace DBClientFiles.NET.Parsing.Versions.WDC1
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
            
            Head = new Segment {
                Identifier = SegmentIdentifier.Header,
                Length = Unsafe.SizeOf<Header>(),

                Handler = new HeaderHandler(this)
            };

            var fieldInfoHandler = new FieldInfoHandler<MemberMetadata>();
            var tail = Head.Next = new Segment {
                Identifier = SegmentIdentifier.FieldInfo,
                Length = Header.FieldInfo.Length,

                Handler = fieldInfoHandler
            };

            if (Header.OffsetMap.Exists)
            {
                tail = tail.Next = new Segment {
                    Identifier = SegmentIdentifier.OffsetMap,
                    Length = Header.OffsetMap.Length,

                    Handler = new OffsetMapHandler()
                };
            }
            else
            {
                tail = tail.Next = new Segment {
                    Identifier = SegmentIdentifier.Records,
                    Length = Header.RecordSize * Header.RecordCount,
                };

                tail = tail.Next = new Segment {
                    Identifier = SegmentIdentifier.StringBlock,
                    Length = Header.StringTable.Length,

                    Handler = new StringBlockHandler(Options.InternStrings)
                };
            }

            tail = tail.Next = new Segment {
                Identifier = SegmentIdentifier.IndexTable,
                Length = Header.IndexTable.Length,

                Handler = new IndexTableHandler()
            };

            tail = tail.Next = new Segment {
                Identifier = SegmentIdentifier.CopyTable,
                Length = Header.CopyTable.Length,

                Handler = new CopyTableHandler()
            };

            tail = tail.Next = new Segment {
                Identifier = SegmentIdentifier.ExtendedFieldInfo,
                Length = Header.ExtendedFieldInfo.Length,

                Handler = new ExtendedFieldInfoHandler<MemberMetadata>(fieldInfoHandler)
            };

            tail = tail.Next = new Segment {
                Identifier = SegmentIdentifier.PalletTable,
                Length = Header.Pallet.Length
            };

            tail = tail.Next = new Segment {
                Identifier = SegmentIdentifier.CommonDataTable,
                Length = Header.Common.Length,

                // Handler = not the legacy one!
            };

            tail = tail.Next = new Segment
            {
                Identifier = SegmentIdentifier.RelationshipTable,
                Length = Header.RelationshipTable.Length
            };
        }

        public override void After(ParsingStep step)
        {
            if (step == ParsingStep.Segments)
                _recordReader = new UnalignedRecordReader(this, Header.RecordSize);
        }
    }
}
