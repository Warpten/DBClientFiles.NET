using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Collections.Generic
{
    public sealed class StorageList<T> : StorageList<int, T>
        where T : class, new()
    {
        public StorageList(Stream dataStream) : base(dataStream)
        {
        }

        public StorageList(Stream dataStream, StorageOptions options) : base(dataStream, options)
        {
        }
    }

    public class StorageList<TKey, T> : IStorage, IList<T>, IList, IReadOnlyList<T>
        where T : class, new()
        where TKey : struct
    {
        private readonly List<T> _container = new List<T>();

        public StorageList(Stream dataStream) : this(dataStream, StorageOptions.Default)
        {
        }

        public StorageList(Stream dataStream, StorageOptions options)
        {
            using (var implementation = new StorageImpl<T>(dataStream, options))
            {
                implementation.InitializeReader<TKey>();
                implementation.ReadHeader();
                foreach (var element in implementation.Enumerate())
                    _container.Add(element);

                Signature = implementation.Signature;
                TableHash = implementation.TableHash;
                LayoutHash = implementation.LayoutHash;
            }
        }

        #region IStorage
        public Signatures Signature { get; }
        public uint TableHash { get; }
        public uint LayoutHash { get; }
        #endregion

        #region IList<T> implementation
        public T this[int index] {
            get => _container[index];
            set => _container[index] = value;
        }

        public bool IsReadOnly => ((IList<T>)_container).IsReadOnly;
        public int Count => _container.Count;
        public void Add(T item) => _container.Add(item);
        public void Clear() => _container.Clear();
        public bool Contains(T item) => _container.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _container.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _container.GetEnumerator();
        public int IndexOf(T item) => _container.IndexOf(item);
        public void Insert(int index, T item) => _container.Insert(index, item);
        public bool Remove(T item) => _container.Remove(item);
        public void RemoveAt(int index) => _container.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => ((IList<T>)_container).GetEnumerator();
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
