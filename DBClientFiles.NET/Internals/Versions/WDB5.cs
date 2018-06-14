using DBClientFiles.NET.Exceptions;
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
    internal class WDB5<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TKey : struct
        where TValue : class, new()
    {
        #region Segments
        private readonly CopyTableReader<TKey> _copyTable;
        #endregion

        protected CodeGenerator<TValue, TKey> _codeGenerator;
        public override CodeGenerator<TValue> Generator => _codeGenerator;
        
        #region Life and death
        public WDB5(IFileHeader header, Stream strm, StorageOptions options) : base(header, strm, options)
        {
            _copyTable = new CopyTableReader<TKey>(this);
        }

        protected override void ReleaseResources()
        {
            base.ReleaseResources();

            _copyTable.Dispose();
        }
        #endregion

        public override bool PrepareMemberInformations()
        {
            Debug.Assert(BaseStream.Position == 48);

            for (var i = 0; i < Header.FieldCount; ++i)
                MemberStore.AddFileMemberInfo(this);

            #region Initialize segments
            Records.StartOffset = BaseStream.Position;
            Records.Length = Header.RecordSize * Header.RecordCount;

            StringTable.StartOffset = Records.EndOffset;
            StringTable.Length = Header.StringTableLength;
            StringTable.Exists = !Header.HasOffsetMap;

            OffsetMap.StartOffset = Header.StringTableLength;
            OffsetMap.Length = (Header.MaxIndex - Header.MinIndex + 1) * (4 + 2);
            OffsetMap.Exists = Header.HasOffsetMap;

            long _foreignIdsLength = Header.HasForeignIds ? (Header.MaxIndex - Header.MinIndex + 1) * 4 : 0;

            IndexTable.StartOffset = (OffsetMap.Exists ? OffsetMap.EndOffset : StringTable.EndOffset) + _foreignIdsLength;
            IndexTable.Length = Header.RecordCount * 4;
            IndexTable.Exists = Header.HasIndexTable;

            _copyTable.StartOffset = IndexTable.EndOffset;
            _copyTable.Length = Header.CopyTableLength;
            #endregion

            _codeGenerator = new CodeGenerator<TValue, TKey>(this);
            return true;
        }

        public override void ReadSegments()
        {
            base.ReadSegments();

            OffsetMap.Read();
            _copyTable.Read();
        }

        protected override IEnumerable<TValue> ReadRecords(int recordIndex, long recordOffset, int recordSize)
        {
            BaseStream.Seek(recordOffset, SeekOrigin.Begin);

            TValue oldStructure;
            using (var recordReader = new RecordReader(this, StringTable.Exists, recordSize))
            {
                oldStructure = IndexTable.Exists
                    ? _codeGenerator.Deserialize(this, recordReader, IndexTable.GetValue<TKey>(recordIndex))
                    : _codeGenerator.Deserialize(this, recordReader);
            }
            
            var sourceID = _codeGenerator.ExtractKey(oldStructure);
            if (_copyTable.ContainsKey(sourceID))
            {
                foreach (var copyEntryID in _copyTable[sourceID])
                {
                    var clone = _codeGenerator.Clone(oldStructure);
                    _codeGenerator.InsertKey(clone, copyEntryID);

                    yield return clone;
                }
            }

            yield return oldStructure;
        }

        public override T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new UnreachableCodeException("WDB5 does not need to implement ReadPalletMember.");
        }

        public override T ReadCommonMember<T>(int memberIndex, TValue value)
        {
            throw new UnreachableCodeException("WDB5 does not need to implement ReadPalletMember.");
        }

        public override T ReadForeignKeyMember<T>()
        {
            throw new UnreachableCodeException("WDB5 does not need to implement ReadForeignKeyMember.");
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new UnreachableCodeException("WDB5 does not need to implement ReadPalletArrayMember.");
        }
    }
}
