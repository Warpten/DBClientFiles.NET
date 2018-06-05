using System;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDBC<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        public WDBC(Stream strm, StorageOptions options) : base(strm, options)
        {
        }

        protected override void ReleaseResources()
        {
            StringTable.Dispose();
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
            Records.ItemLength = recordSize;

            StringTable.Length = stringTableSize;
            StringTable.StartOffset = Records.EndOffset;

            var declaredMemberCount = MemberStore.DeclaredMemberCount();
            if (declaredMemberCount != fieldCount)
                throw new InvalidOperationException($"{typeof(TValue).FullName} declares {declaredMemberCount} members (including arrays), but there should only be {fieldCount}");

            // sets up a default generator
            return base.ReadHeader();
        }

        protected override IEnumerable<TValue> ReadRecords(int recordIndex, long recordOffset, int recordSize)
        {
            using (var segmentStream = new RecordReader(this, StringTable.Exists, recordSize))
                yield return Generator.Deserialize(this, segmentStream);
        }

        public override T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value)
        {
            throw new UnreachableCodeException("WDBC does not need to implement ReadPalletMember.");
        }

        public override T ReadCommonMember<T>(int memberIndex, TValue value)
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
