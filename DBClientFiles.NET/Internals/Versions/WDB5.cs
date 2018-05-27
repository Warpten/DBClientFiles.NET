using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.IO;
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
        private Segment<TValue, IndexTableReader<TKey, TValue>> _indexTable;

        public override Segment<TValue> Records { get; }
        public override Segment<TValue, StringTableReader<TValue>> StringTable { get; }
        public override Segment<TValue> IndexTable => _indexTable;
        public override Segment<TValue> CopyTable => _copyTable;

        protected CodeGenerator<TValue, TKey> _serializer;
        private int _recordSize;

        public WDB5(Stream strm) : base(strm, true)
        {
            _copyTable = new Segment<TValue, CopyTableReader<TKey, TValue>>(this);
            _indexTable = new Segment<TValue, IndexTableReader<TKey, TValue>>(this);

            Records = new Segment<TValue>();
            StringTable = new Segment<TValue, StringTableReader<TValue>>(this);

            _serializer = new CodeGenerator<TValue, TKey>(ValueMembers);
        }

        public override bool ReadHeader()
        {
            var recordCount = ReadInt32();
            if (recordCount == 0)
                return false;

            var fieldCount       = ReadInt32();
            var recordSize       = ReadInt32();
            var stringTableSize  = ReadInt32();
            var tableHash        = ReadInt32();
            var layoutHash       = ReadInt32();
            var minIndex         = ReadInt32();
            var maxIndex         = ReadInt32();
            var locale           = ReadInt32();
            var copyTableSize    = ReadInt32();
            var flags            = ReadInt16();
            var indexColumn      = ReadInt16();

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
            IndexTable.StartOffset = OffsetMap.EndOffset;
            IndexTable.Length = recordCount * 4;

            CopyTable.StartOffset = IndexTable.EndOffset;
            CopyTable.Length = copyTableSize;

            FieldCount = fieldCount;

            _recordSize = recordSize;
            _serializer.IsIndexStreamed = IndexTable.Exists;
            _serializer.IndexColumn = indexColumn;
            return true;
        }

        public override IEnumerable<TValue> ReadRecords()
        {
            var i = 0;
            BaseStream.Position = Records.StartOffset;
            while (BaseStream.Position < Records.EndOffset)
            {
                TValue oldStructure;
                using (var recordReader = new RecordReader(this, StringTable.Exists, _recordSize))
                    oldStructure = IndexTable.Exists ? _serializer.Deserialize(this, recordReader, _indexTable.Reader[i]) : _serializer.Deserialize(this, recordReader);

                BaseStream.Position = OffsetMap.Reader[i];
                var currentKey = _serializer.ExtractKey(oldStructure);

                foreach (var copyEntry in _copyTable.Reader[currentKey])
                {
                    var clone = _serializer.Clone(oldStructure);
                    _serializer.InsertKey(clone, copyEntry);

                    yield return clone;
                }

                yield return oldStructure;

                ++i;
            }
        }

        public override T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new UnreachableCodeException("WDB5 does not need to implement ReadPalletMember.");
        }

        public override T ReadCommonMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new UnreachableCodeException("WDB5 does not need to implement ReadPalletMember.");
        }

        public override T ReadForeignKeyMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new UnreachableCodeException("WDB5 does not need to implement ReadForeignKeyMember.");
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new UnreachableCodeException("WDB5 does not need to implement ReadPalletArrayMember.");
        }
    }
}
