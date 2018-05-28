using System;
using System.Collections.Generic;
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
            internal class WDC2RecordReader : RecordReader
            {
                public WDC2RecordReader(FileReader fileReader, bool usesStringTable, int recordSize) : base(fileReader, usesStringTable, recordSize)
                {
                }

                public override string ReadString()
                {
                    // Part one of adjusting the value read to be a relative offset from the field's start offset.
                    if (_usesStringTable)
                        return _fileReader.FindStringByOffset(StartOffset + _byteCursor / 8 + ReadInt32());

                    return base.ReadString();
                }

                public override string ReadString(int bitOffset, int bitCount)
                {
                    if (_usesStringTable)
                        return _fileReader.FindStringByOffset(_byteCursor / 8 + StartOffset + ReadInt32(bitOffset, bitCount));

                    if ((bitOffset & 7) == 0)
                        return _fileReader.ReadString();

                    throw new InvalidOperationException("Packed strings must be in the string block!");
                }
            }

            private WDC2<TKey, TValue> _parent;

            public override StorageOptions Options { get; set; }

            private Segment<TValue, IndexTableReader<TKey, TValue>> _indexTable;
            private Segment<TValue, CopyTableReader<TKey, TValue>> _copyTable;
            private Segment<TValue, RelationShipSegmentReader<TKey, TValue>> _relationshipData;

            public override Segment<TValue> Records { get; }
            public override Segment<TValue, StringTableReader<TValue>> StringTable { get; }
            public override Segment<TValue, OffsetMapReader<TValue>> OffsetMap { get; }
            public override Segment<TValue> IndexTable => _indexTable;
            public override Segment<TValue> CopyTable => _copyTable;

            public override ExtendedMemberInfo[] Members
            {
                get => _parent.Members;
                protected set => throw new InvalidOperationException();
            }

            private int _unkHeader0;
            private int _unkHeader1;
            private int _fileOffset;
            private int _recordCount;
            private int _stringTableSize;
            private int _copyTableSize;
            private int _offsetMapOffset;
            private int _indexListSize;
            private int _relationshipDataSize;

            private int _currentlyIteratedIndex;

            public Section(WDC2<TKey, TValue> parent, Stream strm) : base(strm)
            {
                _parent = parent;

                _indexTable = new Segment<TValue, IndexTableReader<TKey, TValue>>(this);
                _copyTable = new Segment<TValue, CopyTableReader<TKey, TValue>>(this);
                _relationshipData = new Segment<TValue, RelationShipSegmentReader<TKey, TValue>>(this);

                Records = new Segment<TValue>();
                StringTable = new Segment<TValue, StringTableReader<TValue>>(this);
                OffsetMap = new Segment<TValue, OffsetMapReader<TValue>>(this);

                Options = _parent.Options;
            }

            protected override void ReleaseResources()
            {
                base.ReleaseResources();

                Records.Dispose();
                StringTable.Dispose();
                OffsetMap.Dispose();
                IndexTable.Dispose();
                CopyTable.Dispose();
            }

            public override bool ReadHeader()
            {
                _unkHeader0           = ReadInt32();
                _unkHeader1           = ReadInt32();
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
                if ((_parent._flags & 0x1) == 0)
                {
                    Records.StartOffset = _fileOffset;
                    Records.Length = _recordCount * _parent._recordSize;

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

                CopyTable.StartOffset = IndexTable.EndOffset;
                CopyTable.Length = _copyTableSize;

                _relationshipData.StartOffset = CopyTable.EndOffset;
                _relationshipData.Length = _relationshipDataSize;
            }

            public override void ReadSegments()
            {
                StringTable.Reader.Read();
                _indexTable.Reader.Read();
                _copyTable.Reader.Read();
                _relationshipData.Reader.Read();
            }

            private IEnumerable<TValue> ReadIndividualNodes(int recordSize)
            {
                using (var recordReader = new WDC2RecordReader(_parent, StringTable.Exists, recordSize))
                {
                    var instance = IndexTable.Exists
                        ? _parent.Generator.Deserialize(_parent, recordReader, _indexTable.Reader[_currentlyIteratedIndex])
                        : _parent.Generator.Deserialize(_parent, recordReader);

                    foreach (var copyInstanceID in _copyTable.Reader[_parent.Generator.ExtractKey<TKey>(instance)])
                    {
                        var cloneInstance = _parent.Generator.Clone(instance);
                        _parent.Generator.InsertKey(cloneInstance, copyInstanceID);
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

                        foreach (var node in ReadIndividualNodes(OffsetMap.Reader.GetRecordSize(_currentlyIteratedIndex)))
                            yield return node;
                    }
                }
                else
                {
                    BaseStream.Seek(Records.StartOffset, SeekOrigin.Begin);

                    _currentlyIteratedIndex = 0;
                    while (BaseStream.Position < Records.EndOffset)
                    {
                        foreach (var node in ReadIndividualNodes(_parent._recordSize))
                            yield return node;

                        ++_currentlyIteratedIndex;
                    }
                }
            }

            public override T ReadForeignKeyMember<T>()
            {
                return _relationshipData.Reader.GetForeignKey<T>(_currentlyIteratedIndex);
            }

            public override T ReadCommonMember<T>(int memberIndex, RecordReader recordReader, TValue value)
            {
                throw new UnreachableCodeException("WDC2's section does not contain a common block!");
            }

            public override T[] ReadPalletArrayMember<T>(int memberIndex, RecordReader recordReader, TValue value)
            {
                throw new UnreachableCodeException("WDC2's section does not contain a pallet block!");
            }

            public override T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value)
            {
                throw new UnreachableCodeException("WDC2's section does not contain a pallet block!");
            }

            public override string FindStringByOffset(int tableOffset)
            {
                // Part 2 of string handling: convert the absolute offset into a relative one
                var adjustedPos = tableOffset - StringTable.StartOffset;
                return base.FindStringByOffset((int)adjustedPos);
            }

        }

        #region Segments
        private Section[] _segments;

        private Segment<TValue, BinarySegmentReader<TValue>> _palletTable;
        private Segment<TValue> _commonTable;
        #endregion

        private int _flags;
        private int _recordSize;
        private int _currentlyParsedSegment;

        private CodeGenerator<TValue, TKey>[] _codeGenerator;

        public override CodeGenerator<TValue> Generator
        {
            get
            {
                if (_segments[_currentlyParsedSegment].IndexTable.Exists)
                    return _codeGenerator[0];
                return _codeGenerator[1];
            }
        }

        #region Life and Death
        public WDC2(Stream strm) : base(strm, true)
        {
            _palletTable = new Segment<TValue, BinarySegmentReader<TValue>>(this);
            _commonTable = new Segment<TValue>();
        }

        protected override void ReleaseResources()
        {
            base.ReleaseResources();
        
            _palletTable.Dispose();
            _commonTable.Dispose();

            _codeGenerator = null;

            for (var i = 0; i < _segments.Length; ++i)
                _segments[i].Dispose();
        }
        #endregion

        public override bool ReadHeader()
        {
            var recordCount          = ReadInt32();
            var fieldCount           = ReadInt32();
            var recordSize           = ReadInt32();
            var stringTableSize      = ReadInt32(); // All sections combined
            var tableHash            = ReadInt32();
            var layoutHash           = ReadInt32();
            var minIndex             = ReadInt32();
            var maxIndex             = ReadInt32();
            var locale               = ReadInt32();
            var flags                = ReadInt16();
            var indexColumn          = ReadInt16();
            var totalFieldCount      = ReadInt32();
            var bitpackedDataOffset  = ReadInt32();
            var lookupColumnCount    = ReadInt32();
            var fieldStorageInfoSize = ReadInt32();
            var commonDataSize       = ReadInt32();
            var palletDataSize       = ReadInt32();
            var sectionCount         = ReadInt32();

            _flags = flags;
            _recordSize = recordSize;

            _segments = new Section[sectionCount];
            for (var i = 0; i < _segments.Length; ++i)
            {
                _segments[i] = new Section(this, BaseStream);

                if (!_segments[i].ReadHeader())
                    return false;
            }
            var previousPosition = 0;
            for (var i = 0; i < totalFieldCount; ++i)
            {
                var columnOffset = i;

                var bitSize = ReadInt16();
                var recordPosition = ReadInt16();

                Members[columnOffset].BitSize = 32 - bitSize;
                if (columnOffset > 0 && Members[columnOffset - 1].BitSize != 0)
                    Members[columnOffset - 1].Cardinality = (recordPosition - previousPosition) / Members[columnOffset - 1].BitSize;

                previousPosition = recordPosition;
            }

            for (var i = 0; i < (fieldStorageInfoSize / (2 + 2 + 4 + 4 + 3 * 4)); ++i)
            {
                var columnOffset = i;
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

            for (var i = 0; i < sectionCount; ++i)
                _segments[i].PopulateSegmentOffsets();

            _codeGenerator = new []
            {
                new CodeGenerator<TValue, TKey>(Members) { IsIndexStreamed = false },
                new CodeGenerator<TValue, TKey>(Members) { IsIndexStreamed = true, IndexColumn = indexColumn }
            };
            return true;
        }

        public override void ReadSegments()
        {
            _palletTable.Reader.Read();
            // _commonTable.Reader.Read();

            for (var i = 0; i < _segments.Length; ++i)
                _segments[i].ReadSegments();
        }

        public override IEnumerable<TValue> ReadRecords()
        {
            for (_currentlyParsedSegment = 0; _currentlyParsedSegment < _segments.Length; ++_currentlyParsedSegment)
            {
                var currentSection = _segments[_currentlyParsedSegment];
                foreach (var record in currentSection.ReadRecords())
                    yield return record;
            }
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            var memberInfo = Members[memberIndex];

            return _palletTable.Reader.ReadArray<T>(memberInfo.OffsetInRecord, memberInfo.Cardinality);
        }

        public override T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            var memberInfo = Members[memberIndex];

            return _palletTable.Reader.Read<T>(memberInfo.OffsetInRecord);
        }

        public override T ReadCommonMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new NotImplementedException();
        }

        public override T ReadForeignKeyMember<T>()
        {
            return _segments[_currentlyParsedSegment].ReadForeignKeyMember<T>();
        }

        public override string FindStringByOffset(int tableOffset)
        {
            // Forward the call to the current segment
            return _segments[_currentlyParsedSegment].FindStringByOffset(tableOffset);
        }
    }
}
