using DBClientFiles.NET.Parsing.File;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using DBClientFiles.NET.Collections.Generic.Internal;

namespace DBClientFiles.NET.Collections.Generic
{
    public sealed class StorageEnumerable<T> : IEnumerable<T>
    {
        private Collection<T> _implementation;

        public int Size => _implementation.Size;

        public ref readonly IFileHeader Header => ref _implementation.Header;
        public ref readonly StorageOptions Options => ref _implementation.Options;

        public StorageEnumerable(in StorageOptions options, Stream dataStream)
        {
            _implementation = new Collection<T>(in options, dataStream);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _implementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

}
