using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDB5<TKey, TValue> : BaseReader<TKey, TValue>
        where TKey : struct
        where TValue : class, new()
    {
        public WDB5(Stream strm) : base(strm, true)
        {
            CopyTable = new Segment<TValue, CopyTableReader<TKey, TValue>>(this);
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

            var copyTableSize = ReadInt32();
            var flags = ReadInt16();
            var indexColumn = ReadInt16();

            StringTable.Exists = (flags & 0x01) == 0;
            OffsetMap.Exists = (flags & 0x01) != 0;
            OffsetMap.Length = (maxIndex - minIndex + 1) * (4 + 2);
            if (OffsetMap.Exists)
                OffsetMap.StartOffset = StringTable.Length;

            IndexTable.Exists = (flags & 0x04) != 0;
            IndexTable.StartOffset = OffsetMap.EndOffset;
            IndexTable.Length = recordCount * 4;

            CopyTable = new Segment<TValue, CopyTableReader<TKey, TValue>>(this);
            CopyTable.Exists = copyTableSize != 0;
            CopyTable.Length = copyTableSize;
            CopyTable.StartOffset = IndexTable.EndOffset;

            // TODO: Check that the mapped index column corresponds to metadata

            return true;
        }

        public override IEnumerable<TValue> ReadRecords()
        {
            var serializer = new LegacySerializer<TKey, TValue>(this);
            var cp = (Segment<TValue, CopyTableReader<TKey, TValue>>)CopyTable;

            var i = 0;
            BaseStream.Position = Records.StartOffset;
            while (BaseStream.Position < Records.EndOffset)
            {
                var oldStructure = serializer.Deserialize();

                BaseStream.Position = OffsetMap.Reader[i++];
                var currentKey = serializer.ExtractKey(oldStructure);

                foreach (var copyEntry in cp.Reader[currentKey])
                {
                    var clone = serializer.Clone(oldStructure);
                    serializer.InsertKey(clone, copyEntry);
                    yield return clone;
                }

                yield return oldStructure;
            }
        }
    }
}
