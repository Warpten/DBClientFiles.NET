using System;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDB2<TValue> : BaseFileReader<TValue> where TValue : class, new()
    {
        public override Segment<TValue, StringTableReader<TValue>> StringTable { get; }
        public override Segment<TValue> Records { get; }

        public WDB2(Stream fileStream) : base(fileStream, true)
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
            var stringBlockSize = ReadInt32();
            var tableHash = ReadInt32();
            var buildHash = ReadInt32();
            var timeStampLastWritten = ReadInt32();
            var minIndex = ReadInt32();
            var maxIndex = ReadInt32();
            var locale = ReadInt32();
            var copyTableSize = ReadInt32(); // Unused

            // Skip string length information (unused by nearly everyone)
            if (maxIndex != 0)
                BaseStream.Position += (maxIndex - minIndex + 1) * (4 + 2);

            // Set up segments
            Records.StartOffset = BaseStream.Position;
            Records.Length = recordSize * recordCount;

            StringTable.StartOffset = Records.EndOffset;
            StringTable.Length = stringBlockSize;

            FieldCount = fieldCount;

            return true;
        }

        public override IEnumerable<TValue> ReadRecords()
        {
            var serializer = new CodeGenerator<TValue>(ValueMembers);

            BaseStream.Position = Records.StartOffset;
            while (BaseStream.Position < Records.EndOffset)
                yield return serializer.Deserialize(this);
        }

        public override T ReadPalletMember<T>(int memberIndex, TValue value)
        {
            throw new UnreachableCodeException("WDB2 does not need to implement ReadPalletMember.");
        }

        public override T ReadCommonMember<T>(int memberIndex, TValue value)
        {
            throw new UnreachableCodeException("WDB2 does not need to implement ReadPalletMember.");
        }

        public override T ReadForeignKeyMember<T>(int memberIndex, TValue value)
        {
            throw new UnreachableCodeException("WDB2 does not need to implement ReadForeignKeyMember.");
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex, TValue value)
        {
            throw new UnreachableCodeException("WDB2 does not need to implement ReadPalletArrayMember.");
        }
    }
}
