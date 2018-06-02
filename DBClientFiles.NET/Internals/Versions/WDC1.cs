using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.IO;
using System;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDC1<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        #region Segments
        private readonly BinarySegmentReader _palletTable;
        private readonly CopyTableReader<TKey> _copyTable;
        private readonly RelationShipSegmentReader<TKey> _relationshipData;
        private readonly CommonTableReader<TKey> _commonTable;
        #endregion

        private int _currentRecordIndex = 0;

        private CodeGenerator<TValue, TKey> _codeGenerator;
        public override CodeGenerator<TValue> Generator => _codeGenerator;

        #region Life and death
        public WDC1(Stream strm) : base(strm, true)
        {
            _palletTable      = new BinarySegmentReader(this);
            _copyTable        = new CopyTableReader<TKey>(this);
            _relationshipData = new RelationShipSegmentReader<TKey>(this);
            _commonTable      = new CommonTableReader<TKey>(this);
        }

        protected override void ReleaseResources()
        {
            base.ReleaseResources();
            
            _codeGenerator = null;
            _palletTable.Dispose();
            _copyTable.Dispose();
            _relationshipData.Dispose();
            _commonTable.Dispose();
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
            BaseStream.Seek(4 + 4 + 4, SeekOrigin.Current); // total_field_count, bitpacked_data_ofs, lookup_column_count
            var offsetMapOffset      = ReadInt32();
            var idListSize           = ReadInt32();
            var fieldStorageInfoSize = ReadInt32();
            var commonDataSize       = ReadInt32();
            var palletDataSize       = ReadInt32();
            var relationshipDataSize = ReadInt32();

            MemberStore.IndexColumn = indexColumn;
            IndexTable.Length = idListSize;

            for (var i = 0; i < fieldCount; ++i)
                MemberStore.AddFileMemberInfo(this);

            #region Initialize the first set of segments
            if ((flags & 0x01) == 0)
            {
                Records.StartOffset = BaseStream.Position;
                Records.Length = recordSize * recordCount;
                Records.ItemLength = recordSize;

                StringTable.StartOffset = Records.EndOffset;
                StringTable.Length = stringTableSize;

                OffsetMap.Exists = false;
                IndexTable.StartOffset = StringTable.EndOffset;
            }
            else
            {
                Records.StartOffset = BaseStream.Position;
                Records.Length = (int)(offsetMapOffset - BaseStream.Position);

                OffsetMap.StartOffset = Records.EndOffset;
                OffsetMap.Length = (4 + 2) * (maxIndex - minIndex + 1);

                StringTable.Exists = false;
                IndexTable.StartOffset = OffsetMap.EndOffset;
            }

            _copyTable.StartOffset = IndexTable.EndOffset;
            _copyTable.Length = copyTableSize;
            #endregion
            
            BaseStream.Position = _copyTable.EndOffset;
            var fieldStorageInfoCount = fieldStorageInfoSize / (2 + 2 + 4 + 4 + 3 * 4);
            for (var i = 0; i < fieldStorageInfoCount; ++i)
                MemberStore.FileMembers[i].ReadExtra(this);
            
            #region Initialize the last segments
            _palletTable.StartOffset = BaseStream.Position;
            _palletTable.Length = palletDataSize;

            _commonTable.StartOffset = _palletTable.EndOffset;
            _commonTable.Length = commonDataSize;

            _relationshipData.StartOffset = _commonTable.EndOffset;
            _relationshipData.Length = relationshipDataSize;
            #endregion

            _codeGenerator = new CodeGenerator<TValue, TKey>(this)
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

            _commonTable.Initialize(MemberStore.GetBlockLengths(MemberCompressionType.CommonData));
        }

        public override T ReadCommonMember<T>(int memberIndex, TValue value)
        {
            var memberInfo = MemberStore.FileMembers[memberIndex];

            return _commonTable.ExtractValue(memberInfo.CategoryIndex, memberInfo.GetDefaultValue<T>(), _codeGenerator.ExtractKey(value));
        }

        public override T ReadForeignKeyMember<T>()
        {
            return _relationshipData.GetForeignKey<T>(_currentRecordIndex);
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            var memberInfo = MemberStore.FileMembers[memberIndex];

            var palletOffset = recordReader.ReadBits(memberInfo.Offset, memberInfo.BitSize);
            return _palletTable.ReadArray<T>((int)palletOffset, memberInfo.Cardinality);
        }

        public override T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            var memberInfo = MemberStore.FileMembers[memberIndex];

            var palletOffset = recordReader.ReadBits(memberInfo.Offset, memberInfo.BitSize);
            return _palletTable.Read<T>((int)palletOffset);
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
                    ? _codeGenerator.Deserialize(this, recordReader, IndexTable[recordIndex])
                    : _codeGenerator.Deserialize(this, recordReader);

                foreach (var copyInstanceID in _copyTable[_codeGenerator.ExtractKey(instance)])
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
