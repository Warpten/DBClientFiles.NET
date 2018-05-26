using DBClientFiles.NET.Internals.Segments;
using System;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDC1<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        protected WDC1(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
            Pallet = new Segment<TValue>(this);
            RelationshipData = new Segment<TValue>(this);
        }

        private Segment<TValue> RelationshipData { get; set; }
        private Segment<TValue> Pallet { get; set; }

        public override bool ReadHeader()
        {
            var recordCount = ReadInt32();
            if (recordCount == 0)
                return true;

            var fieldCount           = ReadInt32(); // counts arrays as 1 now, thankfully
            var recordSize           = ReadInt32();
            var stringTableSize      = ReadInt32();
            var tableHash            = ReadInt32();
            var layoutHash           = ReadInt32();
            var minIndex             = ReadInt32();
            var maxIndex             = ReadInt32();
            var locale               = ReadInt32();
            var copyTableSize        = ReadInt32();
            var flags                = ReadInt16();
            var indexColumn          = ReadInt16(); // this is the index of the field containing ID values; this is ignored if flags & 0x04 != 0
            var totalFieldCount      = ReadInt32(); // from WDC1 onwards, this value seems to always be the same as the 'field_count' value
            var bitpackedDataOfs     = ReadInt32(); // relative position in record where bitpacked data begins; not important for parsing the file
            var lookupColumnCount    = ReadInt32(); 
            var offsetMapOffset      = ReadInt32();
            var idListSize           = ReadInt32();
            var fieldStorageInfoSize = ReadInt32();
            var commonDataSize       = ReadInt32();
            var palletDataSize       = ReadInt32();
            var relationshipDataSize = ReadInt32();

            if (ValueMembers.Length != totalFieldCount)
                throw new InvalidOperationException($"Missing column(s) in definition: found {ValueMembers.Length}, expected {totalFieldCount}");

            var previousPosition = 0;
            for (var i = 0; i < totalFieldCount; ++i)
            {
                var bitSize = ReadInt16();
                var recordPosition = ReadInt16();

                ValueMembers[i].BitSize = 4 - bitSize / 8;
                if (i > 0)
                    ValueMembers[i - 1].Cardinality = (recordPosition - previousPosition) / ValueMembers[i - 1].BitSize;

                previousPosition = recordPosition;
            }

            if ((flags & 0x01) == 0)
            {
                Records.StartOffset = BaseStream.Position;
                Records.Length = recordSize * recordCount;

                StringTable.StartOffset = Records.EndOffset;
                StringTable.Length = stringTableSize;

                OffsetMap.Exists = false;
            }
            else
            {
                Records.StartOffset = BaseStream.Position;
                Records.Length = offsetMapOffset - BaseStream.Position;

                OffsetMap.StartOffset = Records.EndOffset;
                OffsetMap.Length = (4 + 2) * (maxIndex - minIndex + 1);

                StringTable.Exists = false;
            }

            IndexTable.StartOffset = ((flags & 0x01) != 0) ? OffsetMap.EndOffset : StringTable.EndOffset;
            IndexTable.Length = idListSize;

            CopyTable.StartOffset = IndexTable.EndOffset;
            CopyTable.Length = copyTableSize;

            for (var i = 0; i < (fieldStorageInfoSize / (2 + 2 + 4 + 4 + 3 * 4)); ++i)
            {
                var fieldOffsetBits = ReadInt16();
                var fieldSizeBits = ReadInt16(); // size is the sum of all array pieces in bits - for example, uint32[3] will appear here as '96'

                var additionalDataSize = ReadInt32();
                var compressionType = (MemberCompressionType)ReadInt32();
                switch (compressionType)
                {
                    case MemberCompressionType.Bitpacked:
                        {
                            BaseStream.Seek(4 + 4, SeekOrigin.Current);
                            var memberFlags = ReadInt32();
                            ValueMembers[i].IsSigned = (memberFlags & 0x01) != 0;
                            break;
                        }
                    case MemberCompressionType.CommonData:
                        ValueMembers[i].DefaultValue = ReadBytes(4);
                        BaseStream.Seek(4 + 4, SeekOrigin.Current);
                        break;
                    case MemberCompressionType.BitpackedPalletData:
                    case MemberCompressionType.BitpackedPalletArrayData:
                        {
                            BaseStream.Seek(4 + 4, SeekOrigin.Current);
                            if (compressionType == MemberCompressionType.BitpackedPalletArrayData)
                                ValueMembers[i].Cardinality = ReadInt32();
                            else
                                BaseStream.Seek(4, SeekOrigin.Current);
                            break;
                        }
                    default:
                        BaseStream.Seek(4 + 4 + 4, SeekOrigin.Current);
                        break;
                }
                ValueMembers[i].Cardinality = fieldSizeBits / ValueMembers[i].BitSize;
            }

            Pallet.StartOffset = BaseStream.Position;
            Pallet.Length = palletDataSize;

            CommonTable.StartOffset = Pallet.EndOffset;
            CommonTable.Length = commonDataSize;

            RelationshipData.StartOffset = CommonTable.EndOffset;
            RelationshipData.Length = relationshipDataSize;

            return true;
        }

        public override void ReadSegments()
        {
            base.ReadSegments();

            // read pallet, common, and relationship
        }

        public override T ReadCommonMember<T>(int memberIndex)
        {
            throw new NotImplementedException();
        }

        public override T ReadForeignKeyMember<T>(int memberIndex)
        {
            throw new NotImplementedException();
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex)
        {
            throw new NotImplementedException();
        }

        public override T ReadPalletMember<T>(int memberIndex)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<TValue> ReadRecords()
        {
            throw new NotImplementedException();
        }
    }
}
