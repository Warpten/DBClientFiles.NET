using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDB2<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        public WDB2(IFileHeader header, Stream fileStream, StorageOptions options) : base(header, fileStream, options)
        {
        }

        protected override void ReleaseResources()
        {
        }

        public override bool PrepareMemberInformations()
        {
            if (Header.MaxIndex == 0)
                Debug.Assert(BaseStream.Position == 48);
            else
                Debug.Assert(BaseStream.Position == 48 + (Header.MaxIndex - Header.MinIndex + 1) * (4 + 2));

            Records.StartOffset = BaseStream.Position;
            Records.Length = Header.RecordSize * Header.RecordCount;

            StringTable.StartOffset = Records.EndOffset;
            StringTable.Length = Header.StringTableLength;

            return base.PrepareMemberInformations();
        }

        protected override IEnumerable<TValue> ReadRecords(int recordIndex, long recordOffset, int recordSize)
        {
            using (var segmentReader = new RecordReader(this, StringTable.Exists, recordSize))
                yield return Generator.Deserialize(this, segmentReader);
        }

        public override T ReadPalletMember<T>(int memberIndex, RecordReader segmentReader, TValue value)
        {
            throw new UnreachableCodeException("WDB2 does not need to implement ReadPalletMember.");
        }

        public override T ReadCommonMember<T>(int memberIndex, TValue value)
        {
            throw new UnreachableCodeException("WDB2 does not need to implement ReadPalletMember.");
        }

        public override T ReadForeignKeyMember<T>()
        {
            throw new UnreachableCodeException("WDB2 does not need to implement ReadForeignKeyMember.");
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex, RecordReader segmentReader, TValue value)
        {
            throw new UnreachableCodeException("WDB2 does not need to implement ReadPalletArrayMember.");
        }
    }
}
