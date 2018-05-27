using DBClientFiles.NET.Internals.Segments;
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
        private Segment<TValue, CopyTableReader<TKey, TValue>> _copyTable;
        private Segment<TValue, IndexTableReader<TKey, TValue>> _indexTable;
        private Segment<TValue, LegacyCommonTableReader<TKey, TValue>> _commonTable;

        public override Segment<TValue> Records { get; }
        public override Segment<TValue, StringTableReader<TValue>> StringTable { get; }
        public override Segment<TValue> IndexTable => _indexTable;
        public override Segment<TValue> CopyTable => _copyTable;
        public override Segment<TValue> CommonTable => _commonTable; 

        public WDB6(Stream dataStream) : base(dataStream)
        {
            _copyTable = new Segment<TValue, CopyTableReader<TKey, TValue>>(this);
            _indexTable = new Segment<TValue, IndexTableReader<TKey, TValue>>(this);
            _commonTable = new Segment<TValue, LegacyCommonTableReader<TKey, TValue>>(this);

            Records = new Segment<TValue>();
            StringTable = new Segment<TValue, StringTableReader<TValue>>(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _commonTable.Dispose();
        }

        public override bool ReadHeader()
        {
            var recordCount = ReadInt32();
            if (recordCount == 0)
                return false;

            var fieldCount = ReadInt32();
            var recordSize = ReadInt32();
            var stringTableSize = ReadInt32();
            var tableHash = ReadInt32();
            var layoutHash = ReadInt32();
            var minIndex = ReadInt32();
            var maxIndex = ReadInt32();
            var locale = ReadInt32();
            var copyTableSize = ReadInt32();
            var flags = ReadInt16();
            var indexColumn = ReadInt32();
            var totalFieldCount = ReadInt32();
            var commonDataTableSize = ReadInt32();

            _serializer.IndexColumn = indexColumn;
            _serializer.IsIndexStreamed = (flags & 0x04) == 0;

            var previousPosition = 0;
            for (var i = 0; i < fieldCount; ++i)
            {
                var bitSize = 32 - ReadInt16();
                var position = ReadInt16();

                ValueMembers[i].BitSize = bitSize;
                if (i > 0)
                    ValueMembers[i - 1].Cardinality = (position - previousPosition) / ValueMembers[i - 1].BitSize;

                previousPosition = position;
            }

            Records.StartOffset = BaseStream.Position;
            Records.Length = recordSize * recordCount;

            StringTable.Exists = (flags & 0x01) == 0;
            StringTable.StartOffset = Records.EndOffset;
            StringTable.Length = stringTableSize;

            OffsetMap.Exists = (flags & 0x01) != 0;
            OffsetMap.StartOffset = stringTableSize;
            OffsetMap.Length = (maxIndex - minIndex + 1) * (4 + 2);

            IndexTable.Exists = (flags & 0x04) != 0;
            IndexTable.StartOffset = OffsetMap.Exists ? OffsetMap.EndOffset : StringTable.EndOffset;
            IndexTable.Length = recordCount * typeof(TKey).GetBinarySize();

            CopyTable.StartOffset = IndexTable.EndOffset;
            CopyTable.Length = copyTableSize;

            CommonTable.StartOffset = CopyTable.EndOffset;
            CommonTable.Length = commonDataTableSize;

            // TODO: Check that the mapped index column corresponds to metadata

            return true;
        }

        public override T ReadCommonMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            return _commonTable.Reader.ExtractValue<T>(memberIndex /* adjust to base-0 for the first column in common */, default /* fixme */);
        }
    }
}
