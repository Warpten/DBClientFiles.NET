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
        private Segment<TValue, BinarySegmentReader<TValue>> _palletTable;
        private Segment<TValue, IndexTableReader<TKey, TValue>> _indexTable;
        private Segment<TValue, CopyTableReader<TKey, TValue>> _copyTable;
        private Segment<TValue, RelationShipSegmentReader<TKey, TValue>> _relationshipData;

        public override Segment<TValue> Records { get; }
        public override Segment<TValue, StringTableReader<TValue>> StringTable { get; }
        public override Segment<TValue, OffsetMapReader<TValue>> OffsetMap { get; }
        public override Segment<TValue> CopyTable => _copyTable;
        public override Segment<TValue> IndexTable => _indexTable;
        public override Segment<TValue> CommonTable { get; }
        private Segment<TValue> RelationshipData => _relationshipData;
        private Segment<TValue> Pallet => _palletTable;
        #endregion

        private int _recordSize;
        private int _currentlyIteratedIndex = 0;

        private CodeGenerator<TValue, TKey> _codeGenerator;
        public override CodeGenerator<TValue> Generator => _codeGenerator;

        #region Life and death
        public WDC1(Stream strm) : base(strm, true)
        {
            _indexTable = new Segment<TValue, IndexTableReader<TKey, TValue>>(this);
            _palletTable = new Segment<TValue, BinarySegmentReader<TValue>>(this);
            _copyTable = new Segment<TValue, CopyTableReader<TKey, TValue>>(this);
            _relationshipData = new Segment<TValue, RelationShipSegmentReader<TKey, TValue>>(this);

            Records = new Segment<TValue>();
            StringTable = new Segment<TValue, StringTableReader<TValue>>(this);
            OffsetMap = new Segment<TValue, OffsetMapReader<TValue>>(this);
            CommonTable = new Segment<TValue>();
        }

        protected override void ReleaseResources()
        {
            base.ReleaseResources();
            
            _codeGenerator = null;

            Records.Dispose();
            StringTable.Dispose();
            OffsetMap.Dispose();
            CopyTable.Dispose();
            IndexTable.Dispose();
            CommonTable.Dispose();
            RelationshipData.Dispose();
            Pallet.Dispose();
        }
        #endregion

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

            // if (Members.Length != totalFieldCount)
            //     throw new InvalidOperationException($"Missing column(s) in definition: found {Members.Length}, expected {totalFieldCount}");

            var previousPosition = 0;
            for (var i = 0; i < totalFieldCount; ++i)
            {
                var columnOffset = (idListSize != 0 && i >= indexColumn) ? (i + 1) : i;

                var bitSize = ReadInt16();
                var recordPosition = ReadInt16();
                
                Members[columnOffset].BitSize = 32 - bitSize;
                if (columnOffset > 0 && Members[columnOffset - 1].BitSize != 0)
                    Members[columnOffset - 1].Cardinality = (recordPosition - previousPosition) / Members[columnOffset - 1].BitSize;

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

            Pallet.StartOffset = BaseStream.Position;
            Pallet.Length = palletDataSize;

            CommonTable.StartOffset = Pallet.EndOffset;
            CommonTable.Length = commonDataSize;

            RelationshipData.StartOffset = CommonTable.EndOffset;
            RelationshipData.Length = relationshipDataSize;

            _codeGenerator = new CodeGenerator<TValue, TKey>(Members)
            {
                IndexColumn = indexColumn,
                IsIndexStreamed = !IndexTable.Exists
            };

            _recordSize = recordSize;
            return true;
        }

        public override void ReadSegments()
        {
            base.ReadSegments();

            _palletTable.Reader.Read();
            _indexTable.Reader.Read();
            _relationshipData.Reader.Read();
        }

        public override T ReadCommonMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new NotImplementedException();
        }

        public override T ReadForeignKeyMember<T>()
        {
            return _relationshipData.Reader.GetForeignKey<T>(_currentlyIteratedIndex);
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            var memberInfo = Members[memberIndex];

            var palletOffset = recordReader.ReadBits(memberInfo.OffsetInRecord, memberInfo.BitSize);
            return _palletTable.Reader.ReadArray<T>(palletOffset, memberInfo.Cardinality);
        }

        public override T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            var memberInfo = Members[memberIndex];

            var palletOffset = recordReader.ReadBits(memberInfo.OffsetInRecord, memberInfo.BitSize);
            return _palletTable.Reader.Read<T>(palletOffset);
        }

        private IEnumerable<TValue> ReadRecord(int recordSize)
        {
            using (var recordReader = new RecordReader(this, StringTable.Exists, recordSize))
            {
                var instance = IndexTable.Exists
                    ? _codeGenerator.Deserialize(this, recordReader, _indexTable.Reader[_currentlyIteratedIndex])
                    : _codeGenerator.Deserialize(this, recordReader);

                foreach (var copyInstanceID in _copyTable.Reader[_codeGenerator.ExtractKey(instance)])
                {
                    var cloneInstance = _codeGenerator.Clone(instance);
                    _codeGenerator.InsertKey(cloneInstance, copyInstanceID);
                    yield return cloneInstance;
                }

                yield return instance;
            }
        }

        public override IEnumerable<TValue> ReadRecords()
        {
            if (OffsetMap.Exists)
            {
                for (_currentlyIteratedIndex = 0; _currentlyIteratedIndex < OffsetMap.Reader.Count; ++_currentlyIteratedIndex)
                {
                    BaseStream.Seek(OffsetMap.Reader.GetRecordOffset(_currentlyIteratedIndex), SeekOrigin.Begin);

                    foreach (var node in ReadRecord(OffsetMap.Reader.GetRecordSize(_currentlyIteratedIndex)))
                        yield return node;
                }
            }
            else
            {
                BaseStream.Seek(Records.StartOffset, SeekOrigin.Begin);

                _currentlyIteratedIndex = 0;
                while (BaseStream.Position < Records.EndOffset)
                {
                    foreach (var node in ReadRecord(_recordSize))
                        yield return node;

                    ++_currentlyIteratedIndex;
                }
            }
        }
    }
}
