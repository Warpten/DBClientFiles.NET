using DBClientFiles.NET.Internals.Versions;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;
using System.Collections.Generic;
using System.IO;
using BinaryReader = DBClientFiles.NET.IO.BinaryReader;

namespace DBClientFiles.NET.Internals.Segments
{
    internal class CopyTable : Segment
    {
        public IEnumerable<KeyValuePair<TKey, TKey>> Enumerate<TKey, TValue>(BaseReader<TValue> reader) where TKey : struct where TValue : class, new()
        {
            reader.BaseStream.Position = StartOffset;
            while (reader.BaseStream.Position < EndOffset)
                yield return new KeyValuePair<TKey, TKey>(reader.ReadStruct<TKey>(), reader.ReadStruct<TKey>());
        }
    }
}
