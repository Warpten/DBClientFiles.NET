using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDBC<TValue> : BaseFileReader<TValue> where TValue : class, new()
    {
        public override Segment<TValue, StringTableReader<TValue>> StringTable { get; }
        public override Segment<TValue> Records { get; }

        private int _recordSize;

        public WDBC(Stream fileStream): base(fileStream, true)
        {
            StringTable = new Segment<TValue, StringTableReader<TValue>>(this);
            Records = new Segment<TValue>();
        }

        public override bool ReadHeader()
        {
            var recordCount = ReadInt32();
            if (recordCount == 0)
                return false;

            var fieldCount = ReadInt32();
            var recordSize = ReadInt32();
            var stringTableSize = ReadInt32();

            Records.StartOffset = BaseStream.Position;
            Records.Length = recordSize * recordCount;

            StringTable.Length = stringTableSize;
            StringTable.StartOffset = Records.EndOffset;

            FieldCount = fieldCount;

            _recordSize = recordSize;

            return true;
        }

        public override IEnumerable<TValue> ReadRecords()
        {
            var serializer = new CodeGenerator<TValue>(Members);
            serializer.IndexColumn = 0;
            serializer.IsIndexStreamed = true;

            BaseStream.Position = Records.StartOffset;
            while (BaseStream.Position < Records.EndOffset)
            {
                using (var segmentStream = new RecordReader(this, StringTable.Exists, _recordSize))
                    yield return serializer.Deserialize(this, segmentStream);
            }
        }

        public override T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new UnreachableCodeException("WDBC does not need to implement ReadPalletMember.");
        }

        public override T ReadCommonMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new UnreachableCodeException("WDBC does not need to implement ReadPalletMember.");
        }

        public override T ReadForeignKeyMember<T>()
        {
            throw new UnreachableCodeException("WDBC does not need to implement ReadForeignKeyMember.");
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new UnreachableCodeException("WDBC does not need to implement ReadPalletArrayMember.");
        }
    }
}
