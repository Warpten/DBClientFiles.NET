using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Internals.Binding;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal sealed partial class WDC2<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        private class Section : WDC1<TKey, TValue>
        {
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

                _copyTable = new CopyTableReader<TKey>(this);
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
                _fileOffset = ReadInt32(); // Absolute offset to the beginning of this section
                _recordCount = ReadInt32();
                _stringTableSize = ReadInt32();
                _copyTableSize = ReadInt32();
                _offsetMapOffset = ReadInt32();
                _indexListSize = ReadInt32();
                _relationshipDataSize = ReadInt32();

                return true;
            }

            public void PopulateSegmentOffsets()
            {
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

                MemberStore = new ExtendedMemberInfoCollection(typeof(TValue), _parent.Options);
                MemberStore.HasIndexTable = IndexTable.Exists;
                MemberStore.IndexColumn = _parent.MemberStore.IndexColumn;

                _copyTable.StartOffset = IndexTable.EndOffset;
                _copyTable.Length = _copyTableSize;

                _relationshipData.StartOffset = _copyTable.EndOffset;
                _relationshipData.Length = _relationshipDataSize;
                
            }

            public override void ReadSegments()
            {
                base.ReadSegments();

                MemberStore.MapMembers();
                MemberStore.CalculateCardinalities();

                _copyTable.Read();
                _relationshipData.Read();
            }

            public override string FindStringByOffset(int tableOffset)
            {
                return StringTable[tableOffset];
            }

            public override RecordReader GetRecordReader(int recordSize)
            {
                return new WDC2.RecordReader(this, StringTable.Exists, recordSize);
            }

            protected override IEnumerable<TValue> ReadRecords(int recordIndex, long recordOffset, int recordSize)
            {
                using (var recordReader = GetRecordReader(recordSize))
                {
                    var instance = IndexTable.Exists
                        ? _codeGenerator.Deserialize(_parent, recordReader, IndexTable.GetValue<TKey>(recordIndex))
                        : _codeGenerator.Deserialize(_parent, recordReader);

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
}
