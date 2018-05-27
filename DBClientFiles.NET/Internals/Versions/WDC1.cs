﻿using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;
using System;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDC1<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        private Segment<TValue, RawDataSegmentReader<TValue>> _palletTable;
        private Segment<TValue, IndexTableReader<TKey, TValue>> _indexTable;
        private Segment<TValue, CopyTableReader<TKey, TValue>> _copyTable;
        public override Segment<TValue> IndexTable => _indexTable;

        public override Segment<TValue> Records { get; }
        public override Segment<TValue, StringTableReader<TValue>> StringTable { get; }
        public override Segment<TValue, OffsetMapReader<TValue>> OffsetMap { get; }
        public override Segment<TValue> CopyTable => _copyTable;
        public override Segment<TValue> CommonTable { get; }
        private Segment<TValue> RelationshipData { get; set; }
        private Segment<TValue> Pallet => _palletTable;

        public WDC1(Stream strm) : base(strm, true)
        {

            _indexTable = new Segment<TValue, IndexTableReader<TKey, TValue>>(this);
            _palletTable = new Segment<TValue, RawDataSegmentReader<TValue>>(this);
            _copyTable = new Segment<TValue, CopyTableReader<TKey, TValue>>(this);

            Records = new Segment<TValue>();
            StringTable = new Segment<TValue, StringTableReader<TValue>>(this);
            OffsetMap = new Segment<TValue, OffsetMapReader<TValue>>(this);
            CommonTable = new Segment<TValue>();
            RelationshipData = new Segment<TValue>(this);
        }


        private CodeGenerator<TValue, TKey> _codeGenerator;

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

            // if (ValueMembers.Length != totalFieldCount)
            //     throw new InvalidOperationException($"Missing column(s) in definition: found {ValueMembers.Length}, expected {totalFieldCount}");

            var previousPosition = 0;
            for (var i = 0; i < totalFieldCount; ++i)
            {
                var columnOffset = (IndexTable.Exists && i >= indexColumn) ? (i + 1) : i;

                var bitSize = ReadInt16();
                var recordPosition = ReadInt16();
                
                ValueMembers[columnOffset].BitSize = 32 - bitSize;
                if (columnOffset > 0 && ValueMembers[columnOffset - 1].BitSize != 0)
                    ValueMembers[columnOffset - 1].Cardinality = (recordPosition - previousPosition) / ValueMembers[columnOffset - 1].BitSize;

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

            BaseStream.Position = CopyTable.EndOffset;

            for (var i = 0; i < (fieldStorageInfoSize / (2 + 2 + 4 + 4 + 3 * 4)); ++i)
            {
                var columnOffset = (IndexTable.Exists && i >= indexColumn) ? (i + 1) : i;

                var fieldOffsetBits = ReadInt16();
                var fieldSizeBits = ReadInt16(); // size is the sum of all array pieces in bits - for example, uint32[3] will appear here as '96'
                if (ValueMembers[columnOffset].BitSize == 0)
                    ValueMembers[columnOffset].BitSize = fieldSizeBits;

                var additionalDataSize = ReadInt32();
                ValueMembers[columnOffset].CompressionType = (MemberCompressionType)ReadInt32();
                switch (ValueMembers[columnOffset].CompressionType)
                {
                    case MemberCompressionType.Bitpacked:
                        {
                            BaseStream.Seek(4 + 4, SeekOrigin.Current);
                            var memberFlags = ReadInt32();
                            ValueMembers[columnOffset].IsSigned = (memberFlags & 0x01) != 0;
                            break;
                        }
                    case MemberCompressionType.CommonData:
                        ValueMembers[columnOffset].DefaultValue = ReadBytes(4);
                        BaseStream.Seek(4 + 4, SeekOrigin.Current);
                        break;
                    case MemberCompressionType.BitpackedPalletData:
                    case MemberCompressionType.BitpackedPalletArrayData:
                        {
                            BaseStream.Seek(4 + 4, SeekOrigin.Current);
                            if (ValueMembers[columnOffset].CompressionType == MemberCompressionType.BitpackedPalletArrayData)
                                ValueMembers[columnOffset].Cardinality = ReadInt32();
                            else
                                BaseStream.Seek(4, SeekOrigin.Current);
                            break;
                        }
                    default:
                        BaseStream.Seek(4 + 4 + 4, SeekOrigin.Current);
                        break;
                }

                if (ValueMembers[columnOffset].BitSize != 0)
                    ValueMembers[columnOffset].Cardinality = fieldSizeBits / ValueMembers[columnOffset].BitSize;

                Console.WriteLine($"{ValueMembers[columnOffset].MemberInfo.Name} is at bit {fieldOffsetBits} and occupies {fieldSizeBits} bits");
            }

            Pallet.StartOffset = BaseStream.Position;
            Pallet.Length = palletDataSize;

            CommonTable.StartOffset = Pallet.EndOffset;
            CommonTable.Length = commonDataSize;

            RelationshipData.StartOffset = CommonTable.EndOffset;
            RelationshipData.Length = relationshipDataSize;
            
            _codeGenerator = new CodeGenerator<TValue, TKey>(ValueMembers);
            _codeGenerator.IndexColumn = indexColumn;
            _codeGenerator.IsIndexStreamed = false;
            return true;
        }

        public override void ReadSegments()
        {
            base.ReadSegments();

            _palletTable.Reader.Read();
            _indexTable.Reader.Read();

            // common, relationship...
        }

        public override T ReadCommonMember<T>(int memberIndex, TValue value)
        {
            throw new NotImplementedException();
        }

        public override T ReadForeignKeyMember<T>(int memberIndex, TValue value)
        {
            throw new NotImplementedException();
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex, TValue value)
        {
            var memberInfo = ValueMembers[memberIndex];

            var palletOffset = ReadBits(memberInfo.BitSize);
            return _palletTable.Reader.ReadArray<T>(palletOffset, memberInfo.Cardinality);
        }

        public override T ReadPalletMember<T>(int memberIndex, TValue value)
        {
            var palletOffset = ReadBits(ValueMembers[memberIndex].BitSize);
            return _palletTable.Reader.Read<T>(palletOffset);
        }

        public override IEnumerable<TValue> ReadRecords()
        {
            BaseStream.Seek(Records.StartOffset, SeekOrigin.Begin);
            var itemIndex = 0;
            while (BaseStream.Position < Records.EndOffset)
            {
                if (OffsetMap.Exists)
                    BaseStream.Seek(OffsetMap.Reader[itemIndex], SeekOrigin.Begin);

                if (IndexTable.Exists)
                    yield return _codeGenerator.Deserialize(this, _indexTable.Reader[itemIndex++]);
                else
                    yield return _codeGenerator.Deserialize(this);

                ResetBitReader();
            }
        }
    }
}