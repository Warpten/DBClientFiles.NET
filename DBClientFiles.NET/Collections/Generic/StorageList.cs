using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Collections.Generic
{
    public class StorageList<T> : List<T> where T : class
    {
        public StorageOptions Options { get; }

        public StorageList(StorageOptions options, Stream dataStream)
        {
            Options = options;

            var enumerable = new StorageEnumerable<T>(options, dataStream);
            AddRange(enumerable);
        }
    }
}
