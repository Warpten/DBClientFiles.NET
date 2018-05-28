using DBClientFiles.NET.Internals;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBClientFiles.NET.Collections.Generic
{
    public sealed class StorageDictionary<TKey, TValue> : StorageBase<TValue>, IDictionary<TKey, TValue>
        where TKey : struct
        where TValue : class, new()
    {
        private readonly Func<TValue, TKey> _keyGetter;
        private readonly Dictionary<TKey, TValue> _container = new Dictionary<TKey, TValue>();

        /// <summary>
        /// Create a new dictionary keyed by a specific column.
        /// </summary>
        /// <param name="dataStream">The stream from which to load data.</param>
        /// <param name="options">The storage options to use.</param>
        /// <param name="keyGetter">A custom function used to select the key from the record.</param>
        /// <remarks><paramref name="keyGetter"/> is not to be confused with <see cref="IndexAttribute"/>, which is a decoration purely reserved 
        /// to the metadata parsing and not related to actual data storage.
        /// 
        /// Also note that it is <b>however</b> mandatory (for now) for the key provided by <paramref name="keyGetter"/> to be of the same type than 
        /// the member decoared with <see cref="IndexAttribute"/>. This limitation will hopefully be raised in a later version.
        /// </remarks>
        public StorageDictionary(Stream dataStream, StorageOptions options, Func<TValue, TKey> keyGetter) : this(dataStream, options)
        {
            _keyGetter = keyGetter;

        }

        /// <summary>
        /// Create a new dictionary keyed by a specific column. This constructor uses <see cref="StorageOptions.Default"/>.
        /// </summary>
        /// <param name="dataStream">The stream from which to load data.</param>
        /// <param name="keyGetter">A custom function used to select the key from the record.</param>
        /// <remarks><paramref name="keyGetter"/> is not to be confused with <see cref="IndexAttribute"/>, which is a decoration purely reserved 
        /// to the metadata parsing and not related to actual data storage.
        /// 
        /// Also note that it is <b>however</b> mandatory (for now) for the key provided by <paramref name="keyGetter"/> to be of the same type than 
        /// the member decoared with <see cref="IndexAttribute"/>. This limitation will hopefully be raised in a later version.
        /// </remarks>
        public StorageDictionary(Stream dataStream, Func<TValue, TKey> keyGetter) : this(dataStream, StorageOptions.Default, keyGetter)
        {
        }

        /// <summary>
        /// Create a new dictionary keyed by a specific column.
        /// 
        /// This constructor uses <see cref="StorageOptions.Default"/>.
        /// They key is selected from <see cref="TValue"/>'s <see cref="System.Reflection.PropertyInfo"/> or <see cref="System.Reflection.FieldInfo"/> which is decorated by <see cref="Attributes.IndexAttribute"/>.
        /// If no member is decorated with <see cref="Attributes.IndexAttribute"/>, it is assumed for the key to be the first declared & used member of the record type.
        /// </summary>
        /// <param name="dataStream">The stream from which to load data.</param>
        public StorageDictionary(Stream dataStream) : this(dataStream, StorageOptions.Default)
        {
        }

        /// <summary>
        /// Create a new dictionary keyed by a specific column.
        /// 
        /// This constructor uses <see cref="StorageOptions.Default"/>.
        /// They key is selected from <see cref="TValue"/>'s <see cref="PropertyInfo"/> or <see cref="FieldInfo"/> which is decorated by <see cref="Attributes.IndexAttribute"/>.
        /// If no member is decorated with <see cref="Attributes.IndexAttribute"/>, it is assumed for the key to be the first declared & used member of the record type.
        /// </summary>
        /// <param name="dataStream">The stream from which to load data.</param>
        /// <param name="options">The options with which to load the file.</param>
        public StorageDictionary(Stream dataStream, StorageOptions options)
        {
            FromStream<TKey>(dataStream, options);
        }

        internal override void LoadRecords(IReader<TValue> reader)
        {
            // TODO Avoid instanciating a new serializer here, use a global application cache instead
            var legacySerializer = new CodeGenerator<TValue, TKey>(reader.Members);

            foreach (var record in reader.ReadRecords())
            {
                var recordKey = _keyGetter?.Invoke(record) ?? legacySerializer.ExtractKey(record);

                _container.Add(recordKey, record);
            }
        }

        /// <inheritdoc/>
        public TValue this[TKey key]
        {
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
    }
}
