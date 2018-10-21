using DBClientFiles.NET.Parsing.File;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Collections.Generic
{
    public class StorageEnumerable<T> : IEnumerable<T>
    {
        public StorageOptions Options => _implementation.Options;
        public IHeader Header => _implementation.Header;
        internal int Size => _implementation.Size;

        private StorageBase<T> _implementation;

        public StorageEnumerable(StorageOptions options, Stream dataStream)
        {
            _implementation = new StorageBase<T>(options, dataStream);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var record in _implementation)
                yield return record.Instance;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
