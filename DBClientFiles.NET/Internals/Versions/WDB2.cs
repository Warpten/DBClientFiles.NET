using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDB2<TValue> : BaseFileReader<TValue> where TValue : class, new()
    {
        public WDB2(Stream fileStream) : base(fileStream, true)
        {
        }

        protected override void ReleaseResources()
        {
        }

        public override bool ReadHeader()
        {
            var recordCount = ReadInt32();
            if (recordCount == 0)
                return false;

            BaseStream.Seek(4, SeekOrigin.Current); // field_count
            var recordSize      = ReadInt32();
            StringTable.Length  = ReadInt32();
            TableHash           = ReadUInt32();
            LayoutHash          = ReadUInt32(); // technically build

            BaseStream.Seek(4, SeekOrigin.Current); // timestamp_last_written

            var minIndex = ReadInt32();
            var maxIndex = ReadInt32();

            BaseStream.Seek(4 + 4, SeekOrigin.Current); // locale, copy_table_size

            // Skip string length information (unused by nearly everyone)
            if (maxIndex != 0)
                BaseStream.Position += (maxIndex - minIndex + 1) * (4 + 2);

            // Set up segments
            Records.StartOffset = BaseStream.Position;
            Records.Length = recordSize * recordCount;
            Records.ItemLength = recordSize;

            StringTable.StartOffset = Records.EndOffset;

            return base.ReadHeader();
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

        public override T ReadCommonMember<T>(int memberIndex, RecordReader segmentReader, TValue value)
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
