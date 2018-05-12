using DBClientFiles.NET.Internals;
using DBClientFiles.NET.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBClientFiles.NET.Collections.Generic
{
    /*public class StorageDictionary<TValue> : StorageDictionary<int, TValue>
    {
        public StorageDictionary(Stream fileStream, StorageOptions options) : base(fileStream, options)
        {
        }
    }

    public class StorageDictionary<TKey, TValue> : StorageBase<TValue>, IDictionary<TKey, TValue> where TKey : struct
    {
        private Func<TValue, TKey> _keyGetter;

        private Dictionary<TKey, TValue> _container = new Dictionary<TKey, TValue>();

        public StorageDictionary(Stream fileStream, StorageOptions options)
        {
            _keyGetter = SerializationUtils<TKey, TValue>.GenerateKeyGetter(options);

            if (SizeCache<TKey>.Size != 4)
                throw new InvalidOperationException($@"Type {typeof(TKey).Name} must be 4 bytes of binary data!");

            FromStream<TKey>(fileStream, options);
        }

        internal override void LoadRecords(IReader reader)
        {
            foreach (var record in reader.ReadRecords<TValue>())
                Add(_keyGetter(record), record);
        }

        public TValue this[TKey key] {
            get => _container[key];
            set => _container[key] = value;
        }

        public ICollection<TKey> Keys => _container.Keys;
        public ICollection<TValue> Values => _container.Values;

        public int Count => _container.Count;
        public bool IsReadOnly => ((IDictionary<TKey, TValue>)_container).IsReadOnly;
        public void Add(TKey key, TValue value) => _container.Add(key, value);
        public void Add(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)_container).Add(item);
        public void Clear() => _container.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item) => _container.Contains(item);
        public bool ContainsKey(TKey key) => ((IDictionary<TKey, TValue>)_container).ContainsKey(key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)_container).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _container.GetEnumerator();
        public bool Remove(TKey key) => _container.Remove(key);
        public bool Remove(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)_container).Remove(item);
        public bool TryGetValue(TKey key, out TValue value) => _container.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<TKey, TValue>)_container).GetEnumerator();
    }*/
}
