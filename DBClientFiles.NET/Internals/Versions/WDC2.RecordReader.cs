using System;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal sealed class WDC2
    {
        public class RecordReader : DBClientFiles.NET.IO.RecordReader
        {
            public RecordReader(FileReader fileReader, bool usesStringTable, int recordSize) : base(fileReader, usesStringTable, recordSize)
            {
            }

            public override string ReadString()
            {
                // Part one of adjusting the value read to be a relative offset from the field's start offset.
                if (_usesStringTable)
                    return _fileReader.FindStringByOffset(StartOffset + _bitCursor / 8 + ReadInt32());

                return base.ReadString();
            }

            public override string ReadString(int bitOffset, int bitCount)
            {
                if (_usesStringTable)
                    return _fileReader.FindStringByOffset(_bitCursor / 8 + StartOffset + ReadInt32(bitOffset, bitCount));

                if ((bitOffset & 7) == 0)
                    return _fileReader.ReadString();

                throw new InvalidOperationException("Packed strings must be in the string block!");
            }
        }
    }
}
