using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Parsing.File;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Collections.Generic
{
    public class StorageList<T> : IList<T>, IDisposable
    {
        private IList<T> _impl;

        private StorageOptions _options;
        public ref readonly StorageOptions Options => ref _options;

        public StorageList(in StorageOptions options, Stream dataStream)
        {
            _options = options;

            var enumerable = new StorageEnumerable<T>(options, dataStream);

            _impl = new List<T>(enumerable);
        }

        public void Dispose()
        {
            if (IsReadOnly)
                return;

            // Save changes now
            /// TODO
        }

        public int Count => _impl.Count;
        public bool IsReadOnly => Options.ReadOnly;

        public T this[int index]
        {
            get => _impl[index];
            set
            {
                if (IsReadOnly)
                    throw new ReadOnlyException("This collection is read-only.");

                _impl[index] = value;
            }
        }

        public int IndexOf(T item) => _impl.IndexOf(item);

        public void Insert(int index, T item)
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            _impl.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            _impl.RemoveAt(index);
        }

        public void Add(T item)
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            _impl.Add(item);
        }

        public void Clear()
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            _impl.Clear();
        }

        public bool Contains(T item) => _impl.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _impl.CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            return _impl.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _impl.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _impl.GetEnumerator();
        }
    }
}
