using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBClientFiles.NET.Collections.Generic
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class StorageDictionary<TKey, TValue> : IStorage, IDictionary<TKey, TValue>
        where TKey : struct
        where TValue : class, new()
    {
        #region IStorage
        public Signatures Signature { get; }
        public uint TableHash { get; }
        public uint LayoutHash { get; }
        #endregion

        private readonly Dictionary<TKey, TValue> _container;

        public StorageDictionary(Stream dataStream) : this(dataStream, StorageOptions.Default)
        {
        }

        public StorageDictionary(Stream dataStream, Func<TValue, TKey> keySelector = null) : this(dataStream, StorageOptions.Default, keySelector)
        {
        }

        public StorageDictionary(Stream dataStream, StorageOptions options, Func<TValue, TKey> keySelector = null)
        {
            if (keySelector == null)
            {
                _container = new Dictionary<TKey, TValue>();
                using (var implementation = new StorageImpl<TValue>(dataStream, options))
                {
                    implementation.InitializeHeaderInfo();

                    var indexMember = implementation.Members.IndexMember;
                    if (indexMember.Type != typeof(TKey))
                        throw new InvalidOperationException();

                    if (!typeof(TKey).IsValueType)
                        throw new InvalidOperationException();

                    implementation.InitializeFileReader<TKey>();
                    implementation.PrepareMemberInfo();
                    foreach (var item in implementation.Enumerate())
                        _container[implementation.ExtractKey<TKey>(item)] = item;

                    Signature = implementation.Header.Signature;
                    TableHash = implementation.Header.TableHash;
                    LayoutHash = implementation.Header.LayoutHash;
                }
            }
            else
            {
                var enumerable = new StorageEnumerable<TValue>(dataStream, options);
                Signature = enumerable.Signature;
                TableHash = enumerable.TableHash;
                LayoutHash = enumerable.LayoutHash;

                _container = enumerable.ToDictionary(keySelector);
            }
        }

        #region IDictionary<TKey, TValue> implementation
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
        public bool ContainsKey(TKey key) => _container.ContainsKey(key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)_container).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _container.GetEnumerator();
        public bool Remove(TKey key) => _container.Remove(key);
        public bool Remove(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)_container).Remove(item);
        public bool TryGetValue(TKey key, out TValue value) => _container.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<TKey, TValue>)_container).GetEnumerator();
        #endregion
    }
}
