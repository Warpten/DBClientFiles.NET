using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDC1<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        #region Segments
        private readonly PalletSegmentReader _palletTable;
        private readonly CopyTableReader<TKey> _copyTable;
        private readonly RelationShipSegmentReader<TKey> _relationshipData;
        private readonly CommonTableReader<TKey> _commonTable;
        #endregion

        private int _currentRecordIndex = 0;

        private CodeGenerator<TValue, TKey> _codeGenerator;
        public override CodeGenerator<TValue> Generator => _codeGenerator;

        #region Life and death
        public WDC1(IFileHeader header, Stream strm, StorageOptions options) : base(header, strm, options)
        {
            _palletTable      = new PalletSegmentReader(this);
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

        public override bool PrepareMemberInformations()
        {
            Debug.Assert(BaseStream.Position == 48);

            BaseStream.Seek(4 + 4 + 4, SeekOrigin.Current); // total_field_count, bitpacked_data_ofs, lookup_column_count
            var offsetMapOffset      = ReadInt32();
            var idListSize           = ReadInt32();
            var fieldStorageInfoSize = ReadInt32();
            var commonDataSize       = ReadInt32();
            var palletDataSize       = ReadInt32();
            var relationshipDataSize = ReadInt32();
            
            IndexTable.Length = idListSize;

            for (var i = 0; i < Header.FieldCount; ++i)
                MemberStore.AddFileMemberInfo(this);

            #region Initialize the first set of segments
            if (!Header.HasOffsetMap)
            {
                Records.StartOffset = BaseStream.Position;
                Records.Length = Header.RecordSize * Header.RecordCount;

                StringTable.StartOffset = Records.EndOffset;
                StringTable.Length = Header.StringTableLength;

                OffsetMap.Exists = false;
                IndexTable.StartOffset = StringTable.EndOffset;
            }
            else
            {
                Records.StartOffset = BaseStream.Position;
                Records.Length = (int)(offsetMapOffset - BaseStream.Position);

                OffsetMap.StartOffset = Records.EndOffset;
                OffsetMap.Length = (4 + 2) * (Header.MaxIndex - Header.MinIndex + 1);

                StringTable.Exists = false;
                IndexTable.StartOffset = OffsetMap.EndOffset;
            }

            _copyTable.StartOffset = IndexTable.EndOffset;
            _copyTable.Length = Header.CopyTableLength;
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

            _codeGenerator = new CodeGenerator<TValue, TKey>(this);
            return true;
        }

        public override void ReadSegments()
        {
            base.ReadSegments();
            
            _relationshipData.Read();
            _copyTable.Read();

            _commonTable.Initialize(MemberStore.GetBlockLengths(MemberCompressionType.CommonData));
            _palletTable.Initialize(MemberStore.GetBlockLengths(f =>
                f.CompressionType == MemberCompressionType.BitpackedPalletArrayData ||
                f.CompressionType == MemberCompressionType.BitpackedPalletData));
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
            return _palletTable.ReadArray<T>(memberInfo.CategoryIndex, (int)palletOffset, memberInfo.Cardinality);
        }

        public override T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            var memberInfo = MemberStore.FileMembers[memberIndex];

            var palletOffset = recordReader.ReadBits(memberInfo.Offset, memberInfo.BitSize);
            return _palletTable.Read<T>(memberInfo.CategoryIndex, (int)palletOffset);
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
                    ? _codeGenerator.Deserialize(this, recordReader, IndexTable.GetValue<TKey>(recordIndex))
                    : _codeGenerator.Deserialize(this, recordReader);

                var instanceKey = _codeGenerator.ExtractKey(instance);
                if (_copyTable.ContainsKey(instanceKey))
                {
                    foreach (var copyInstanceID in _copyTable[instanceKey])
                    {
                        var cloneInstance = _codeGenerator.Clone(instance);
                        _codeGenerator.InsertKey(cloneInstance, copyInstanceID);
                        yield return cloneInstance;
                    }
                }

                yield return instance;
            }
        }
    }
}
