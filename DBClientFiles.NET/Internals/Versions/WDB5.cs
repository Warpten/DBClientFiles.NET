using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.IO;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDB5<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TKey : struct
        where TValue : class, new()
    {
        #region Segments
        private readonly CopyTableReader<TKey, TValue> _copyTable;
        #endregion

        protected CodeGenerator<TValue, TKey> _codeGenerator;
        public override CodeGenerator<TValue> Generator => _codeGenerator;
        
        #region Life and death
        public WDB5(Stream strm) : base(strm, true)
        {
            _copyTable = new CopyTableReader<TKey, TValue>(this);
        }

        protected override void ReleaseResources()
        {
            base.ReleaseResources();

            _copyTable.Dispose();
        }
        #endregion

        public override bool ReadHeader()
        {
            var recordCount = ReadInt32();
            if (recordCount == 0)
                return false;

            var fieldCount       = ReadInt32();
            var recordSize       = ReadInt32();
            var stringTableSize  = ReadInt32();
            TableHash            = ReadUInt32();
            LayoutHash           = ReadUInt32();
            var minIndex         = ReadInt32();
            var maxIndex         = ReadInt32();
            BaseStream.Seek(4, SeekOrigin.Current); // locale
            var copyTableSize    = ReadInt32();
            var flags            = ReadInt16();
            var indexColumn      = ReadInt16();

            var previousPosition = 0;
            for (var i = 0; i < fieldCount; ++i)
            {
                var bitSize = 32 - ReadInt16();
                var position = ReadInt16();

                Members[i].BitSize = bitSize;
                if (i > 0)
                    Members[i - 1].Cardinality = (position - previousPosition) / Members[i - 1].BitSize;

                previousPosition = position;
            }

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
            IndexTable.StartOffset = OffsetMap.EndOffset;
            IndexTable.Length = recordCount * 4;

            _copyTable.StartOffset = IndexTable.EndOffset;
            _copyTable.Length = copyTableSize;

            _codeGenerator = new CodeGenerator<TValue, TKey>(Members)
            {
                IndexColumn = indexColumn,
                IsIndexStreamed = !IndexTable.Exists
            };
            return true;
        }
        
        protected override IEnumerable<TValue> ReadRecords(int recordIndex, long recordOffset, int recordSize)
        {
            TValue oldStructure;
            using (var recordReader = new RecordReader(this, StringTable.Exists, recordSize))
            {
                oldStructure = IndexTable.Exists
                    ? _codeGenerator.Deserialize(this, recordReader, IndexTable[recordIndex])
                    : _codeGenerator.Deserialize(this, recordReader);
            }
            
            var sourceID = _codeGenerator.ExtractKey(oldStructure);
            foreach (var copyEntryID in _copyTable[sourceID])
            {
                var clone = _codeGenerator.Clone(oldStructure);
                _codeGenerator.InsertKey(clone, copyEntryID);

                yield return clone;
            }

            yield return oldStructure;
        }
        
        public override T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new UnreachableCodeException("WDB5 does not need to implement ReadPalletMember.");
        }

        public override T ReadCommonMember<T>(int memberIndex, RecordReader recordReader, TValue value)
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
