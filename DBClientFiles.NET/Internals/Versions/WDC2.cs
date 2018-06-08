using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal sealed partial class WDC2<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        #region Segments
        private Section[] _sections;

        private readonly PalletSegmentReader _palletTable;
        private readonly CommonTableReader<TKey> _commonTable;
        #endregion

        private int _currentlyParsedSection;

        public override CodeGenerator<TValue> Generator => _sections[_currentlyParsedSection].Generator;
        
        #region Life and Death
        public WDC2(IFileHeader header, Stream strm, StorageOptions options) : base(header, strm, options)
        {
            _palletTable = new PalletSegmentReader(this);
            _commonTable = new CommonTableReader<TKey>(this);
        }

        protected override void ReleaseResources()
        {
            base.ReleaseResources();
        
            _palletTable.Dispose();
            _commonTable.Dispose();

            for (var i = 0; i < _sections.Length; ++i)
                _sections[i].Dispose();
        }
        #endregion

        public override bool PrepareMemberInformations()
        {
            Debug.Assert(BaseStream.Position == 48);

            var totalFieldCount      = ReadInt32();
            BaseStream.Seek(4 + 4, SeekOrigin.Current); // bitpacked_data_ofs, lookup_column_count
            var fieldStorageInfoSize = ReadInt32();
            var commonDataSize       = ReadInt32();
            var palletDataSize       = ReadInt32();
            var sectionCount         = ReadInt32();

            _sections = new Section[sectionCount];
            for (var i = 0; i < _sections.Length; ++i)
            {
                _sections[i] = new Section(this, BaseStream);

                if (!_sections[i].PrepareMemberInformations())
                    return false;
            }

            for (var i = 0; i < totalFieldCount; ++i)
                MemberStore.AddFileMemberInfo(this);

            var fieldStorageInfoCount = fieldStorageInfoSize / (2 + 2 + 4 + 4 + 3 * 4);
            for (var i = 0; i < fieldStorageInfoCount; ++i)
                MemberStore.FileMembers[i].ReadExtra(this);

            _palletTable.StartOffset = BaseStream.Position;
            _palletTable.Length = palletDataSize;

            _commonTable.StartOffset = _palletTable.EndOffset;
            _commonTable.Length = commonDataSize;

            for (var i = 0; i < sectionCount; ++i)
            {
                _sections[i].PopulateSegmentOffsets();
                _sections[i].SetFileMemberInfo(MemberStore.FileMembers);
            }

            return true;
        }

        public override void ReadSegments()
        {
            _commonTable.Initialize(MemberStore.GetBlockLengths(MemberCompressionType.CommonData));
            _palletTable.Initialize(MemberStore.GetBlockLengths(f =>
                f.CompressionType == MemberCompressionType.BitpackedPalletArrayData ||
                f.CompressionType == MemberCompressionType.BitpackedPalletData));

            for (var i = 0; i < _sections.Length; ++i)
                _sections[i].ReadSegments();
        }

        public override IEnumerable<TValue> ReadRecords()
        {
            if (!Options.LoadMask.HasFlag(LoadMask.Records))
                yield break;

            for (_currentlyParsedSection = 0; _currentlyParsedSection < _sections.Length; ++_currentlyParsedSection)
            {
                var currentSection = _sections[_currentlyParsedSection];
                foreach (var record in currentSection.ReadRecords())
                    yield return record;
            }
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            var memberInfo = MemberStore.FileMembers[memberIndex];

            return _palletTable.ReadArray<T>(memberInfo.CategoryIndex, memberInfo.Offset, memberInfo.Cardinality);
        }

        public override T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            var memberInfo = MemberStore.FileMembers[memberIndex];

            return _palletTable.Read<T>(memberInfo.CategoryIndex, memberInfo.Offset);
        }

        public override T ReadCommonMember<T>(int memberIndex, TValue value)
        {
            var memberInfo = MemberStore.FileMembers[memberIndex];

            return _commonTable.ExtractValue(memberInfo.CategoryIndex, memberInfo.GetDefaultValue<T>(), _sections[_currentlyParsedSection].ExtractRecordKey(value));
        }

        public override T ReadForeignKeyMember<T>()
        {
            return _sections[_currentlyParsedSection].ReadForeignKeyMember<T>();
        }

        public override string FindStringByOffset(int tableOffset)
        {
            return _sections[_currentlyParsedSection].FindStringByOffset(tableOffset);
        }

        protected override IEnumerable<TValue> ReadRecords(int recordIndex, long recordOffset, int recordSize)
        {
            throw new UnreachableCodeException("WDC2.ReadRecords should never execute!");
        }
    }
}
