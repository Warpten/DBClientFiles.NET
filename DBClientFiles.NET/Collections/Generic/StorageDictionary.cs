using DBClientFiles.NET.Types;
using System;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Collections.Generic
{
    public sealed class StorageDictionary<T> : Dictionary<uint, T> where T : class, IKeyType
    {
        public StorageOptions Options { get; }

        public StorageDictionary(StorageOptions options, Stream dataStream)
        {
            Options = options;

            var enumerable = new StorageEnumerable<T>(options, dataStream);
            foreach (var record in enumerable)
                Add(record.ID, record);
        }
    }

    public class StorageDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : class
    {
        public StorageOptions Options { get; }

        public StorageDictionary(StorageOptions options, Stream dataStream, Func<TValue, TKey> keyGetter)
        {
            Options = options;

            var enumerable = new StorageEnumerable<TValue>(options, dataStream);
            foreach (var record in enumerable)
                Add(keyGetter(record), record);
        }
    }
}
