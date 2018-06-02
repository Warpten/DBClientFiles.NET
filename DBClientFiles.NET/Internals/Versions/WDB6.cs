using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;
using System.IO;

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
        public WDB6(Stream dataStream) : base(dataStream)
        {
            _copyTable   = new CopyTableReader<TKey>(this);
            _commonTable = new LegacyCommonTableReader<TKey>(this);
        }

        protected override void ReleaseResources()
        {
            base.ReleaseResources();

            _copyTable.Dispose();
            _commonTable.Dispose();
        }
        #endregion

        public override bool ReadHeader()
        {
            var recordCount = ReadInt32();
            if (recordCount == 0)
                return false;

            var fieldCount          = ReadInt32();
            var recordSize          = ReadInt32();
            var stringTableSize     = ReadInt32();
            TableHash               = ReadUInt32();
            LayoutHash              = ReadUInt32();
            var minIndex            = ReadInt32();
            var maxIndex            = ReadInt32();
            BaseStream.Seek(4, SeekOrigin.Current); // locale
            var copyTableSize       = ReadInt32();
            var flags               = ReadInt16();
            var indexColumn         = ReadInt32();
            BaseStream.Seek(4, SeekOrigin.Current); // total_field_count
            var commonDataTableSize = ReadInt32();

            _commonTableStartColumn = fieldCount;

            #region Initialize segments
            Records.StartOffset = BaseStream.Position;
            Records.Length = recordSize * recordCount;
            Records.ItemLength = recordSize;

            StringTable.Exists = (flags & 0x01) == 0;
            StringTable.StartOffset = Records.EndOffset;
            StringTable.Length = stringTableSize;

            OffsetMap.Exists = (flags & 0x01) != 0;
            OffsetMap.StartOffset = stringTableSize;
            OffsetMap.Length = (maxIndex - minIndex + 1) * (4 + 2);

            IndexTable.Exists = (flags & 0x04) != 0;
            IndexTable.StartOffset = OffsetMap.Exists ? OffsetMap.EndOffset : StringTable.EndOffset;
            IndexTable.Length = recordCount * SizeCache<TKey>.Size;

            _copyTable.StartOffset = IndexTable.EndOffset;
            _copyTable.Length = copyTableSize;

            _commonTable.StartOffset = _copyTable.EndOffset;
            _commonTable.Length = commonDataTableSize;
            #endregion

            for (var i = 0; i < fieldCount; ++i)
                MemberStore.AddFileMemberInfo(this);

            // TODO: Check that the mapped index column corresponds to metadata
            _codeGenerator.IndexColumn = indexColumn;
            _codeGenerator.IsIndexStreamed = !IndexTable.Exists;

            return true;
        }

        public override T ReadCommonMember<T>(int memberIndex,  TValue value)
        {
            return _commonTable.ExtractValue<T>(memberIndex - _commonTableStartColumn, _codeGenerator.ExtractKey(value));
        }
    }
}
