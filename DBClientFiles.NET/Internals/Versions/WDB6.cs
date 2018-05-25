using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using System;
using System.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDB6<TKey, TValue> : WDB5<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        private Segment<TValue, LegacyCommonTableReader<TKey, TValue>> _commonTable;
        public override Segment<TValue> CommonTable => _commonTable; 

        public WDB6(Stream dataStream) : base(dataStream)
        {
            _commonTable = new Segment<TValue, LegacyCommonTableReader<TKey, TValue>>(this);
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

            FieldCount = ReadInt32();
            var recordSize = ReadInt32();
            StringTable.Length = ReadInt32();

            // Table hash; Layout hash
            BaseStream.Position += 4 + 4;
            
            var minIndex = ReadInt32();
            var maxIndex = ReadInt32();

            BaseStream.Position += 4; // Locale

            CopyTable.Length = ReadInt32();
            var flags = ReadInt16();
            var indexColumn = ReadInt32();
            var totalFieldCount = ReadInt32();
            _commonTable.Length = ReadInt32();

            StringTable.Exists = (flags & 0x01) == 0;

            OffsetMap.Exists = (flags & 0x01) != 0;
            OffsetMap.Length = (maxIndex - minIndex + 1) * (4 + 2);
            if (OffsetMap.Exists) // not a typo - if flag is set old length is an absolute offset
                OffsetMap.StartOffset = StringTable.Length;

            IndexTable.Exists = (flags & 0x04) != 0;
            IndexTable.StartOffset = OffsetMap.EndOffset;
            IndexTable.Length = recordCount * 4;

            CopyTable.StartOffset = IndexTable.EndOffset;

            CommonTable.StartOffset = CopyTable.EndOffset;

            // TODO: Check that the mapped index column corresponds to metadata

            return true;
        }

        public override T ReadCommonMember<T>(int memberIndex)
        {
            return _commonTable.Reader.ReadStructValue<T>(memberIndex /* adjust to base-0 for the first column in common */, default /* fixme */);
        }
    }
}
