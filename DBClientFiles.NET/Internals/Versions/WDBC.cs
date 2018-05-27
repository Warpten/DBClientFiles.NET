using System;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDBC<TValue> : BaseFileReader<TValue> where TValue : class, new()
    {
        public override Segment<TValue, StringTableReader<TValue>> StringTable { get; }

        private CodeGenerator<TValue> _deserializer;

        public WDBC(Stream fileStream): base(fileStream, true)
        {
            StringTable = new Segment<TValue, StringTableReader<TValue>>(this);
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

            _deserializer = new CodeGenerator<TValue>(ValueMembers);

            return true;
        }

        public override IEnumerable<TValue> HandleRecord(UnmanagedMemoryStream memoryStream)
        {
            using (var reader = new BitReader(memoryStream))
                yield return _deserializer.Deserialize(reader);
        }

        public override T ReadPalletMember<T>(int memberIndex, TValue value)
        {
            throw new UnreachableCodeException("WDBC does not need to implement ReadPalletMember.");
        }

        public override T ReadCommonMember<T>(int memberIndex, TValue value)
        {
            throw new UnreachableCodeException("WDBC does not need to implement ReadPalletMember.");
        }

        public override T ReadForeignKeyMember<T>(int memberIndex, TValue value)
        {
            throw new UnreachableCodeException("WDBC does not need to implement ReadForeignKeyMember.");
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex, TValue value)
        {
            throw new UnreachableCodeException("WDBC does not need to implement ReadPalletArrayMember.");
        }
    }
}
