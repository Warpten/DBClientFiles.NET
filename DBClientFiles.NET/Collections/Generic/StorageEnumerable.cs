using System.Collections;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Collections.Generic.Internal;
using DBClientFiles.NET.Parsing;
using DBClientFiles.NET.Parsing.Versions;
using DBClientFiles.NET.Utils.Extensions;

namespace DBClientFiles.NET.Collections.Generic
{
    public sealed class StorageEnumerable<T> : IRecordEnumerable<T>
    {
        private readonly IBinaryStorageFile<T> _implementation;

        /// <summary>
        /// The options used to create this collection.
        /// </summary>
        public ref readonly StorageOptions Options => ref _implementation.Options;

        /// <summary>
        /// Constructs a collection from the provided stream.
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

        public void Dispose()
        {
            _implementation.Dispose();
        }

        public IEnumerator<T> GetEnumerator() => _implementation.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IRecordEnumerable<T> Skip(int amount) => new SkippingStorageEnumerable<T>(this, amount);

        public T ElementAt(int offset)
        {
            using (var enumerator = _implementation.GetEnumerator())
                return enumerator.ElementAt(offset);
        }

        public T ElementAtOrDefault(int offset)
        {
            using (var enumerator = _implementation.GetEnumerator())
                return enumerator.ElementAtOrDefault(offset);
        }

        public T Last()
        {
            using (var enumerator = _implementation.GetEnumerator())
                return enumerator.Last();
        }
    }

}
