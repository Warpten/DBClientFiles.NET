using DBClientFiles.NET.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Collections.Generic
{
    public sealed class StorageList<TKey, TValue> : StorageList<TValue> where TKey : struct where TValue : class, new()
    {
        public StorageList(Stream fileStream) : this(fileStream, StorageOptions.Default)
        {

        }

        public StorageList(Stream fileStream, StorageOptions options) : base(fileStream, options)
        {

        }

        protected override void FromStream(Stream fileStream, StorageOptions options)
        {
            FromStream<TKey>(fileStream, options);
        }
    }

    public class StorageList<TValue> : StorageBase<TValue>, IList<TValue>, IList where TValue : class, new()
    {
        private List<TValue> _container = new List<TValue>();

        public StorageList(Stream fileStream) : this(fileStream, StorageOptions.Default)
        {
        }

        public StorageList(Stream fileStream, StorageOptions options)
        {
            FromStream(fileStream, options);
        }

        internal override void LoadRecords(IReader<TValue> reader)
        {
            foreach (var record in reader.ReadRecords())
                _container.Add(record);
        }

        #region IList<TValue> implementation
        public TValue this[int index] {
            get => _container[index];
            set => _container[index] = value;
        }

        public bool IsReadOnly => ((IList<TValue>)_container).IsReadOnly;
        public int Count => _container.Count;
        public void Add(TValue item) => _container.Add(item);
        public void Clear() => _container.Clear();
        public bool Contains(TValue item) => _container.Contains(item);
        public void CopyTo(TValue[] array, int arrayIndex) => _container.CopyTo(array, arrayIndex);
        public IEnumerator<TValue> GetEnumerator() => _container.GetEnumerator();
        public int IndexOf(TValue item) => _container.IndexOf(item);
        public void Insert(int index, TValue item) => _container.Insert(index, item);
        public bool Remove(TValue item) => _container.Remove(item);
        public void RemoveAt(int index) => _container.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => ((IList<TValue>)_container).GetEnumerator();
        #endregion

        #region IList implementation
        public bool IsFixedSize => ((IList)_container).IsFixedSize;
        public object SyncRoot => ((IList)_container).SyncRoot;
        public bool IsSynchronized => ((IList)_container).IsSynchronized;

        object IList.this[int index] {
            get => ((IList)_container)[index];
            set => ((IList)_container)[index] = value;
        }

        public int Add(object value) => ((IList)_container).Add(value);
        public bool Contains(object value) => ((IList)_container).Contains(value);
        public int IndexOf(object value) => ((IList)_container).IndexOf(value);
        public void Insert(int index, object value) => ((IList)_container).Insert(index, value);
        public void Remove(object value) => ((IList)_container).Remove(value);
        public void CopyTo(Array array, int index) => ((IList)_container).CopyTo(array, index);
        #endregion
    }
}
