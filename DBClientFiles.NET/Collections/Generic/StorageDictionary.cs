using DBClientFiles.NET.Collections.Generic.Exceptions;
using DBClientFiles.NET.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Collections.Generic
{
    public sealed class StorageDictionary<T> : IDictionary<uint, T> where T : class, IKeyType
    {
        public StorageOptions Options { get; }

        private IDictionary<uint, T> _impl;

        public StorageDictionary(StorageOptions options, Stream dataStream)
        {
            Options = options;

            var enumerable = new StorageEnumerable<T>(options, dataStream);
            _impl = new Dictionary<uint, T>(enumerable.Size);
            foreach (var record in enumerable)
                _impl.Add(record.ID, record);
        }

        public ICollection<uint> Keys => _impl.Keys;
        public ICollection<T> Values => _impl.Values;

        public int Count => _impl.Count;
        public bool IsReadOnly => Options.ReadOnly;

        public T this[uint key]
        {
            get => _impl[key];
            set
            {
                if (IsReadOnly)
                    throw new ReadOnlyException("This collection is read-only.");

                _impl[key] = value;
            }
        }

        public void Add(uint key, T value)
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            _impl.Add(key, value);
        }

        public bool ContainsKey(uint key) => _impl.ContainsKey(key);
        public bool Remove(uint key)
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            return _impl.Remove(key);
        }

        public bool TryGetValue(uint key, out T value) => _impl.TryGetValue(key, out value);
        public void Add(KeyValuePair<uint, T> item)
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            _impl.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            _impl.Clear();
        }

        public bool Contains(KeyValuePair<uint, T> item) => _impl.Contains(item);
        public void CopyTo(KeyValuePair<uint, T>[] array, int arrayIndex) => _impl.CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<uint, T> item)
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            return _impl.Remove(item);
        }

        public IEnumerator<KeyValuePair<uint, T>> GetEnumerator() => _impl.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _impl.GetEnumerator();
    }

    public class StorageDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        public StorageOptions Options { get; }

        private IDictionary<TKey, TValue> _impl;

        public StorageDictionary(StorageOptions options, Stream dataStream, Func<TValue, TKey> keyGetter)
        {
            Options = options;

            _impl = new Dictionary<TKey, TValue>();

            var enumerable = new StorageEnumerable<TValue>(options, dataStream);
            foreach (var record in enumerable)
                _impl.Add(keyGetter(record), record);
        }

        public ICollection<TKey> Keys => _impl.Keys;
        public ICollection<TValue> Values => _impl.Values;

        public int Count => _impl.Count;
        public bool IsReadOnly => Options.ReadOnly;

        public TValue this[TKey key]
        {
            get => _impl[key];
            set
            {
                if (IsReadOnly)
                    throw new ReadOnlyException("This collection is read-only.");

                _impl[key] = value;
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            _impl.Add(key, value);
        }

        public bool ContainsKey(TKey key) => _impl.ContainsKey(key);
        public bool Remove(TKey key)
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            return _impl.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value) => _impl.TryGetValue(key, out value);
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            _impl.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            _impl.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => _impl.Contains(item);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _impl.CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (IsReadOnly)
                throw new ReadOnlyException("This collection is read-only.");

            return _impl.Remove(item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _impl.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _impl.GetEnumerator();
    }
}
