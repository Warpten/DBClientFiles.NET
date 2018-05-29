using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDC1<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        #region Segments
        private readonly BinarySegmentReader<TValue> _palletTable;
        private readonly CopyTableReader<TKey, TValue> _copyTable;
        private readonly RelationShipSegmentReader<TKey, TValue> _relationshipData;
        private Segment<TValue> _commonTable;
        #endregion

        private int _currentRecordIndex = 0;

        private CodeGenerator<TValue, TKey> _codeGenerator;
        public override CodeGenerator<TValue> Generator => _codeGenerator;

        #region Life and death
        public WDC1(Stream strm) : base(strm, true)
        {
            _palletTable      = new BinarySegmentReader<TValue>(this);
            _copyTable        = new CopyTableReader<TKey, TValue>(this);
            _relationshipData = new RelationShipSegmentReader<TKey, TValue>(this);
        }

        protected override void ReleaseResources()
        {
            base.ReleaseResources();
            
            _codeGenerator = null;
            _palletTable.Dispose();
            _copyTable.Dispose();
            _relationshipData.Dispose();
        }
        #endregion

        public override bool ReadHeader()
        {
            var recordCount = ReadInt32();
            if (recordCount == 0)
                return true;

            var fieldCount           = ReadInt32();
            var recordSize           = ReadInt32();
            var stringTableSize      = ReadInt32();
            TableHash                = ReadUInt32();
            LayoutHash               = ReadUInt32();
            var minIndex             = ReadInt32();
            var maxIndex             = ReadInt32();
            BaseStream.Seek(4, SeekOrigin.Current); // locale
            var copyTableSize        = ReadInt32();
            var flags                = ReadInt16();
            var indexColumn          = ReadInt16(); // this is the index of the field containing ID values; this is ignored if flags & 0x04 != 0
            var totalFieldCount      = ReadInt32(); // from WDC1 onwards, this value seems to always be the same as the 'field_count' value
            BaseStream.Seek(4 + 4, SeekOrigin.Current); // bitpacked_data_ofs, lookup_column_count
            var offsetMapOffset      = ReadInt32();
            var idListSize           = ReadInt32();
            var fieldStorageInfoSize = ReadInt32();
            var commonDataSize       = ReadInt32();
            var palletDataSize       = ReadInt32();
            var relationshipDataSize = ReadInt32();

            // if (Members.Length != totalFieldCount)
            //     throw new InvalidOperationException($"Missing column(s) in definition: found {Members.Length}, expected {totalFieldCount}");

            var previousPosition = 0;
            for (var i = 0; i < Members.Length; ++i)
            {
                var memberInfo = Members[i];
                if (idListSize != 0 && i == indexColumn)
                    continue;

                var previousMember = i - 1;
                if (idListSize != 0 && (i - 1) == indexColumn)
                    previousMember -= 1;

                var bitSize = ReadInt16();
                var recordPosition = ReadInt16();

                memberInfo.BitSize = 32 - bitSize;
                if (previousMember > 0 && Members[previousMember].BitSize != 0)
                    Members[previousMember].Cardinality = (recordPosition - previousPosition) / Members[previousMember].BitSize;

                previousPosition = recordPosition;
            }

            if ((flags & 0x01) == 0)
            {
                Records.StartOffset = BaseStream.Position;
                Records.Length = recordSize * recordCount;
                Records.ItemLength = recordSize;

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

            IndexTable.Exists = idListSize != 0;
            IndexTable.StartOffset = ((flags & 0x01) != 0) ? OffsetMap.EndOffset : StringTable.EndOffset;
            IndexTable.Length = idListSize;

            _copyTable.StartOffset = IndexTable.EndOffset;
            _copyTable.Length = copyTableSize;

            BaseStream.Position = _copyTable.EndOffset;

            for (var i = 0; i < (fieldStorageInfoSize / (2 + 2 + 4 + 4 + 3 * 4)); ++i)
            {
                var columnOffset = (idListSize != 0 && i >= indexColumn) ? (i + 1) : i;
                var memberInfo = Members[columnOffset];

                memberInfo.OffsetInRecord = ReadInt16();
                var fieldSizeBits = ReadInt16(); // size is the sum of all array pieces in bits - for example, uint32[3] will appear here as '96'
                if (memberInfo.BitSize == 0)
                    memberInfo.BitSize = fieldSizeBits;

                var additionalDataSize = ReadInt32();
                memberInfo.CompressionType = (MemberCompressionType)ReadInt32();
                switch (memberInfo.CompressionType)
                {
                    case MemberCompressionType.Immediate:
                        {
                            BaseStream.Seek(4 + 4, SeekOrigin.Current);
                            var memberFlags = ReadInt32();
                            memberInfo.IsSigned = (memberFlags & 0x01) != 0;
                            break;
                        }
                    case MemberCompressionType.CommonData:
                        memberInfo.DefaultValue = ReadBytes(4);
                        BaseStream.Seek(4 + 4, SeekOrigin.Current);
                        break;
                    case MemberCompressionType.BitpackedPalletData:
                    case MemberCompressionType.BitpackedPalletArrayData:
                        {
                            BaseStream.Seek(4 + 4, SeekOrigin.Current);
                            if (memberInfo.CompressionType == MemberCompressionType.BitpackedPalletArrayData)
                                memberInfo.Cardinality = ReadInt32();
                            else
                                BaseStream.Seek(4, SeekOrigin.Current);
                            break;
                        }
                    default:
                        BaseStream.Seek(4 + 4 + 4, SeekOrigin.Current);
                        break;
                }

                if (memberInfo.BitSize != 0)
                    memberInfo.Cardinality = fieldSizeBits / memberInfo.BitSize;
            }

            _palletTable.StartOffset = BaseStream.Position;
            _palletTable.Length = palletDataSize;

            _commonTable.StartOffset = _palletTable.EndOffset;
            _commonTable.Length = commonDataSize;

            _relationshipData.StartOffset = _commonTable.EndOffset;
            _relationshipData.Length = relationshipDataSize;

            _codeGenerator = new CodeGenerator<TValue, TKey>(Members)
            {
                IndexColumn = indexColumn,
                IsIndexStreamed = !IndexTable.Exists
            };
            
            return true;
        }

        public override void ReadSegments()
        {
            base.ReadSegments();

            _palletTable.Read();
            _relationshipData.Read();
        }

        public override T ReadCommonMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new NotImplementedException();
        }

        public override T ReadForeignKeyMember<T>()
        {
            return _relationshipData.GetForeignKey<T>(_currentRecordIndex);
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            var memberInfo = Members[memberIndex];

            var palletOffset = recordReader.ReadBits(memberInfo.OffsetInRecord, memberInfo.BitSize);
            return _palletTable.ReadArray<T>(palletOffset, memberInfo.Cardinality);
        }

        public override T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            var memberInfo = Members[memberIndex];

            var palletOffset = recordReader.ReadBits(memberInfo.OffsetInRecord, memberInfo.BitSize);
            return _palletTable.Read<T>(palletOffset);
        }

        public virtual RecordReader GetRecordReader(int recordSize)
        {
            return new RecordReader(this, StringTable.Exists, recordSize);
        }

        protected override IEnumerable<TValue> ReadRecords(int recordIndex, long recordOffset, int recordSize)
        {
            _currentRecordIndex = recordIndex;
            using (var recordReader = GetRecordReader(recordSize))
            {
                var instance = IndexTable.Exists
                    ? Generator.Deserialize(this, recordReader, IndexTable[recordIndex])
                    : Generator.Deserialize(this, recordReader);

                foreach (var copyInstanceID in _copyTable[Generator.ExtractKey<TKey>(instance)])
                {
                    var cloneInstance = _codeGenerator.Clone(instance);
                    _codeGenerator.InsertKey(cloneInstance, copyInstanceID);
                    yield return cloneInstance;
                }

                yield return instance;
            }
        }
    }
}
