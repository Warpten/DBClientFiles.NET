using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Internals.Serializers
{
    internal class StorageSerializer<TValue> where TValue : struct
    {
        public int ValueSize { get; } = SizeCache<TValue>.Size;

        public Signatures Signature { get; set; } = Signatures.WDBC;

        public void Serialize(Stream targetStream, StorageBase<TValue> storage)
        {
        }
    }
}
