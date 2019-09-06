using System.Collections;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Parsing;
using DBClientFiles.NET.Parsing.Versions;

namespace DBClientFiles.NET.Collections.Generic
{
    public sealed class StorageEnumerable<T> : IEnumerable<T>
    {
        private IBinaryStorageFile<T> _implementation;

        /// <summary>
        /// The options used to create this collection.
        /// </summary>
        public ref readonly StorageOptions Options => ref _implementation.Options;

        /// <summary>
        /// Constructs a collection, given options and a data stream.
        /// </summary>
        /// <param name="options">The options used for loading.</param>
        /// <param name="dataStream">The stream of binary data to load from.</param>
        public StorageEnumerable(in StorageOptions options, Stream dataStream)
        {
            _implementation = BinaryStorageFactory<T>.Process(in options, dataStream);
        }

        /// <summary>
        /// Constructs a collection from the provided stream, using the default configuration from
        /// <see cref="StorageOptions.Default"/>.
        /// </summary>
        /// <param name="dataStream"></param>
        public StorageEnumerable(Stream dataStream) : this(in StorageOptions.Default, dataStream)
        {
        }

        public IEnumerator<T> GetEnumerator() => _implementation.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// TODO: Provide random access in constant time
        /// TODO: Optimize Skip and Take somehow
    }

}
