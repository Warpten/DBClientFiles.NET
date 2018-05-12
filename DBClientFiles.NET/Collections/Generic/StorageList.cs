using DBClientFiles.NET.Internals;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Collections.Generic
{
    public sealed class StorageList<TKey, TValue> : StorageList<TValue> where TKey : struct where TValue : class, new()
    {
        public StorageList(Stream fileStream, StorageOptions options) : base(fileStream, options)
        {

        }

        protected override void FromStream(Stream fileStream, StorageOptions options)
        {
            FromStream<TKey>(fileStream, options);
        }
    }

    public class StorageList<TValue> : StorageBase<TValue>, IList<TValue> where TValue : class, new()
    {
        private List<TValue> _container = new List<TValue>();

        public StorageList(Stream fileStream, StorageOptions options)
        {
            FromStream(fileStream, options);
        }

        internal override void LoadRecords(IReader<TValue> reader)
        {
            foreach (var record in reader.ReadRecords())
                _container.Add(record);
        }

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
    }
}
