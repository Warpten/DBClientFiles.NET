using System;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Internals.Serializers;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDBC<TValue> : BaseFileReader<TValue> where TValue : class, new()
    {
        public WDBC(Stream fileStream): base(fileStream, true)
        {
        }

        public override bool ReadHeader()
        {
            var recordCount = ReadInt32();
            if (recordCount == 0)
                return false;

            FieldCount = ReadInt32();
            var recordSize = ReadInt32();
            StringTable.Length = ReadInt32();

            Records.StartOffset = BaseStream.Position;
            Records.Length = recordSize * recordCount;

            StringTable.StartOffset = Records.EndOffset;

            return true;
        }

        public override IEnumerable<TValue> ReadRecords()
        {
            var serializer = new CodeGenerator<TValue>(ValueMembers);

            BaseStream.Position = Records.StartOffset;
            while (BaseStream.Position < Records.EndOffset)
                yield return serializer.Deserialize(this);

//#if PERFORMANCE
//            DeserializeGeneration = _serializer.DeserializerGeneration;
//#endif
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
