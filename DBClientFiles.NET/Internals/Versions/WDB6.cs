using System.Diagnostics;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Utils;
using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Generators;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDB6<TKey, TValue> : WDB5<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        #region Segments
        private readonly CopyTableReader<TKey> _copyTable;
        private readonly LegacyCommonTableReader<TKey> _commonTable;
        #endregion

        private int _commonTableStartColumn;

        #region Life and death
        public WDB6(IFileHeader header, Stream strm, StorageOptions options) : base(header, strm, options)
        {
            _copyTable = new CopyTableReader<TKey>(this);
            _commonTable = new LegacyCommonTableReader<TKey>(this);
        }

        protected override void ReleaseResources()
        {
            base.ReleaseResources();

            _copyTable.Dispose();
            _commonTable.Dispose();
        }
        #endregion

        public override bool PrepareMemberInformations()
        {
            Debug.Assert(BaseStream.Position == 48);

            BaseStream.Seek(4, SeekOrigin.Current); // total_field_count
            var commonDataTableSize = ReadInt32();

            _commonTableStartColumn = Header.FieldCount;

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
            IndexTable.Length = Header.RecordCount * SizeCache<TKey>.Size;
            IndexTable.Exists = Header.HasIndexTable;

            _copyTable.StartOffset = IndexTable.EndOffset;
            _copyTable.Length = Header.CopyTableLength;

            _commonTable.StartOffset = _copyTable.EndOffset;
            _commonTable.Length = commonDataTableSize;
            #endregion

            _codeGenerator = new CodeGenerator<TValue, TKey>(this);

            return true;
        }

        public override T ReadCommonMember<T>(int memberIndex, TValue value)
        {
            return _commonTable.ExtractValue<T>(memberIndex - _commonTableStartColumn, _codeGenerator.ExtractKey(value));
        }
    }
}
