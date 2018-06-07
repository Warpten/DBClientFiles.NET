using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Versions
{
    internal sealed class WDC2<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        private class Section : WDC1<TKey, TValue>
        {
            private class WDC2RecordReader : RecordReader
            {
                public WDC2RecordReader(FileReader fileReader, bool usesStringTable, int recordSize) : base(fileReader, usesStringTable, recordSize)
                {
                }

                public override string ReadString()
                {
                    // Part one of adjusting the value read to be a relative offset from the field's start offset.
                    if (_usesStringTable)
                        return _fileReader.FindStringByOffset(StartOffset + _bitCursor / 8 + ReadInt32());

                    return base.ReadString();
                }

                public override string ReadString(int bitOffset, int bitCount)
                {
                    if (_usesStringTable)
                        return _fileReader.FindStringByOffset(_bitCursor / 8 + StartOffset + ReadInt32(bitOffset, bitCount));

                    if ((bitOffset & 7) == 0)
                        return _fileReader.ReadString();

                    throw new InvalidOperationException("Packed strings must be in the string block!");
                }
            }

            private readonly WDC2<TKey, TValue> _parent;
            
            private readonly CopyTableReader<TKey> _copyTable;
            private readonly RelationShipSegmentReader<TKey> _relationshipData;

            private readonly CodeGenerator<TValue, TKey> _codeGenerator;
            public override CodeGenerator<TValue> Generator => _codeGenerator;

            private int _fileOffset;
            private int _recordCount;
            private int _stringTableSize;
            private int _copyTableSize;
            private int _offsetMapOffset;
            private int _indexListSize;
            private int _relationshipDataSize;

            public Section(WDC2<TKey, TValue> parent, Stream strm) : base(parent.Header, strm, parent.Options)
            {
                _parent = parent;
                
                _copyTable        = new CopyTableReader<TKey>(this);
                _relationshipData = new RelationShipSegmentReader<TKey>(this);

                _codeGenerator = new CodeGenerator<TValue, TKey>(this);
            }

            protected override void ReleaseResources()
            {
                base.ReleaseResources();

                _copyTable.Dispose();
                _relationshipData.Dispose();
            }

            public TKey ExtractRecordKey(TValue instance) => _codeGenerator.ExtractKey(instance);

            public void SetFileMemberInfo(IEnumerable<FileMemberInfo> fileMembers)
            {
                MemberStore.SetFileMemberInfo(fileMembers);
            }

            public override bool PrepareMemberInformations()
            {
                BaseStream.Seek(4 + 4, SeekOrigin.Current); // unk_header[2]
                _fileOffset           = ReadInt32(); // Absolute offset to the beginning of this section
                _recordCount          = ReadInt32();
                _stringTableSize      = ReadInt32();
                _copyTableSize        = ReadInt32();
                _offsetMapOffset      = ReadInt32();
                _indexListSize        = ReadInt32();
                _relationshipDataSize = ReadInt32();
                
                return true;
            }

            public void PopulateSegmentOffsets()
            {
                MemberStore = new ExtendedMemberInfoCollection(typeof(TValue), _parent.Options);

                if (!Header.HasOffsetMap)
                {
                    Records.StartOffset = _fileOffset;
                    Records.Length = _recordCount * Header.RecordSize;

                    StringTable.StartOffset = Records.EndOffset;
                    StringTable.Length = _stringTableSize;

                    IndexTable.StartOffset = StringTable.EndOffset;
                }
                else
                {
                    OffsetMap.StartOffset = _fileOffset;
                    OffsetMap.Length = _offsetMapOffset - _fileOffset;

                    IndexTable.StartOffset = OffsetMap.EndOffset;
                }

                IndexTable.Length = _indexListSize;

                _copyTable.StartOffset = IndexTable.EndOffset;
                _copyTable.Length = _copyTableSize;

                _relationshipData.StartOffset = _copyTable.EndOffset;
                _relationshipData.Length = _relationshipDataSize;
            
            }

            public override void ReadSegments()
            {
                base.ReadSegments();

                _copyTable.Read();
                _relationshipData.Read();
            }

            public override string FindStringByOffset(int tableOffset)
            {
                // Part 2 of string handling: convert the absolute offset into a relative one
                var adjustedPos = tableOffset - StringTable.StartOffset;
                return base.FindStringByOffset((int)adjustedPos);
            }

            public override RecordReader GetRecordReader(int recordSize)
            {
                return new WDC2RecordReader(this, StringTable.Exists, recordSize);
            }

            protected override IEnumerable<TValue> ReadRecords(int recordIndex, long recordOffset, int recordSize)
            {
                using (var recordReader = GetRecordReader(recordSize))
                {
                    var instance = IndexTable.Exists
                        ? _codeGenerator.Deserialize(this, recordReader, IndexTable.GetValue<TKey>(recordIndex))
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
            _palletTable.Read();
            _commonTable.Read();

            for (var i = 0; i < _sections.Length; ++i)
                _sections[i].ReadSegments();
        }

        public override IEnumerable<TValue> ReadRecords()
        {
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

            return _commonTable.ExtractValue(memberInfo.CategoryIndex, 
                memberInfo.GetDefaultValue<T>(),
                _sections[_currentlyParsedSection].ExtractRecordKey(value)); //! TODO FIXME
        }

        public override T ReadForeignKeyMember<T>()
        {
            return _sections[_currentlyParsedSection].ReadForeignKeyMember<T>();
        }

        public override string FindStringByOffset(int tableOffset)
        {
            // Forward the call to the current segment
            return _sections[_currentlyParsedSection].FindStringByOffset(tableOffset);
        }

        protected override IEnumerable<TValue> ReadRecords(int recordIndex, long recordOffset, int recordSize)
        {
            throw new UnreachableCodeException("WDC2.ReadRecords should never execute!");
        }
    }
}
