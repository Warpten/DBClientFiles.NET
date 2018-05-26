using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;
using System;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDB5<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TKey : struct
        where TValue : class, new()
    {
        private Segment<TValue, CopyTableReader<TKey, TValue>> _copyTable;
        public override Segment<TValue> CopyTable => _copyTable;

        public WDB5(Stream strm) : base(strm, true)
        {
            _copyTable = new Segment<TValue, CopyTableReader<TKey, TValue>>(this);
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

            CopyTable.Length = copyTableSize;
            CopyTable.StartOffset = IndexTable.EndOffset;

            // TODO: Check that the mapped index column corresponds to metadata

            return true;
        }

        public override IEnumerable<TValue> ReadRecords()
        {
            var serializer = new CodeGenerator<TValue, TKey>(ValueMembers);

            var i = 0;
            BaseStream.Position = Records.StartOffset;
            while (BaseStream.Position < Records.EndOffset)
            {
                var oldStructure = serializer.Deserialize(this);

                BaseStream.Position = OffsetMap.Reader[i++];
                var currentKey = serializer.ExtractKey(ref oldStructure);

                foreach (var copyEntry in _copyTable.Reader[currentKey])
                {
                    var clone = serializer.Clone(oldStructure);
                    serializer.InsertKey(ref clone, copyEntry);

                    yield return clone;
                }

                yield return oldStructure;
            }
        }

        public override T ReadPalletMember<T>(int memberIndex)
        {
            throw new InvalidOperationException();
        }

        public override T ReadCommonMember<T>(int memberIndex)
        {
            throw new InvalidOperationException();
        }

        public override T ReadForeignKeyMember<T>(int memberIndex)
        {
            throw new InvalidOperationException();
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex)
        {
            throw new InvalidOperationException();
        }
    }
}
