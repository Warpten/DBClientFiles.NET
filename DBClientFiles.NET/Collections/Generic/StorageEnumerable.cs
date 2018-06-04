using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Collections.Generic
{
    public sealed class StorageEnumerable<T> : StorageEnumerable<int, T>
        where T : class, new()
    {
        public StorageEnumerable(Stream fileStream) : base(fileStream)
        {
        }

        public StorageEnumerable(Stream fileStream, StorageOptions options) : base(fileStream, options)
        {
        }
    }

    /// <summary>
    /// An enumerable storage representation of dbc and db2 files.
    /// </summary>
    /// <typeparam name="TKey">The key type declared by the file.</typeparam>
    /// <typeparam name="T">The element type.</typeparam>
    public class StorageEnumerable<TKey, T> : IStorage, IEnumerable<T>
        where T : class, new()
        where TKey : struct
    {
        #region IStorage
        public Signatures Signature { get; }
        public uint TableHash { get; }
        public uint LayoutHash { get; }
        #endregion

        private StorageImpl<T> _implementation;
        private IEnumerable<T> _enumerable;

        public StorageEnumerable(Stream fileStream) : this(fileStream, StorageOptions.Default)
        {
        }

        ~StorageEnumerable()
        {
            _enumerable = null;
            _implementation.Dispose();
        }

        public StorageEnumerable(Stream fileStream, StorageOptions options)
        {
            _implementation = new StorageImpl<T>(fileStream, options);
            _implementation.InitializeReader<TKey>();
            _implementation.ReadHeader();
            _enumerable = _implementation.Enumerate();

            Signature = _implementation.Signature;
            TableHash = _implementation.TableHash;
            LayoutHash = _implementation.LayoutHash;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_enumerable == null)
                throw new ObjectDisposedException("StorageEnumerable<T>");

            return _enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_enumerable == null)
                throw new ObjectDisposedException("StorageEnumerable<T>");

            return _enumerable.GetEnumerator();
        }
    }
}
