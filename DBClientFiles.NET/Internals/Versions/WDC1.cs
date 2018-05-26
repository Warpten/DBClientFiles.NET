using System;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDC1<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        protected WDC1(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
        }

        public override bool ReadHeader()
        {
            throw new NotImplementedException();
        }

        public override T ReadCommonMember<T>(int memberIndex)
        {
            throw new NotImplementedException();
        }

        public override T ReadForeignKeyMember<T>(int memberIndex)
        {
            throw new NotImplementedException();
        }

        public override T[] ReadPalletArrayMember<T>(int memberIndex)
        {
            throw new NotImplementedException();
        }

        public override T ReadPalletMember<T>(int memberIndex)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<TValue> ReadRecords()
        {
            throw new NotImplementedException();
        }
    }
}
