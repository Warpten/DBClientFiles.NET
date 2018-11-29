using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.File.WDB5
{
    internal sealed class Parser<T> : BinaryFileParser<T, Serializer<T>>
    {
        private IFileHeader _fileHeader;
        public override ref readonly IFileHeader Header => ref _fileHeader;

        public override int RecordCount => _fileHeader.RecordCount + _fileHeader.CopyTableLength / 2;

        private BaseMemberMetadata[] FileMembers;

        public Parser(in StorageOptions options, Stream input) : base(options, input)
        {
            _fileHeader = new Header(this);

            RegisterBlockHandler(new FieldStructureBlockHandler());
            RegisterBlockHandler(new IndexTableHandler<int>());
        }

        protected override IRecordReader GetRecordReader()
        {
            return null;
            // return new AlignedRecordReader(this, Header
        }

        protected override void Prepare()
        {
            FileMembers = new BaseMemberMetadata[Header.FieldCount];

            var tail = Head.Next = new Block {
                Identifier = BlockIdentifier.FieldInfo,
                Length = Header.FieldCount * (2 + 2)
            };

            tail = tail.Next = new Block {
                Identifier = BlockIdentifier.Records,
                Length = Header.HasOffsetMap
                    ? Header.StringTableLength - tail.EndOffset
                    : Header.RecordCount * Header.RecordSize
            };

            if (!Header.HasOffsetMap)
            {
                tail = tail.Next = new Block
                {
                    Identifier = BlockIdentifier.StringBlock,
                    Length = Header.StringTableLength
                };

                RegisterBlockHandler(new StringBlockHandler(Options.InternStrings));
            }
            else
            {
                tail = tail.Next = new Block
                {
                    Identifier = BlockIdentifier.OffsetMap,
                    Length = (4 + 2) * (Header.MaxIndex - Header.MinIndex + 1)
                };
            }

            if (Header.HasForeignIds)
            {
                tail = tail.Next = new Block
                {
                    Identifier = BlockIdentifier.RelationShipTable,
                    Length = 4 * (Header.MaxIndex - Header.MinIndex + 1)
                };
            }

            if (Header.CopyTableLength > 0)
            {
                tail = tail.Next = new Block
                {
                    Identifier = BlockIdentifier.CopyTable,
                    Length = Header.CopyTableLength
                };

                RegisterBlockHandler(new CopyTableHandler<int>());
            }
        }

        public override BaseMemberMetadata GetFileMemberMetadata(int index)
            => FileMembers[index];
    }
}
