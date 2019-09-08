using DBClientFiles.NET.Parsing.Versions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers
{
    internal abstract class ListBlockHandler<TElement> : IList<TElement>, ISegmentHandler
    {
        private readonly List<TElement> _store;
        
        protected ListBlockHandler()
        {
            _store = new List<TElement>();
        }

        protected ListBlockHandler(List<TElement> store)
        {
            _store = store;
        }

        public void ReadSegment(IBinaryStorageFile reader, long startOffset, long length)
        {
            if (length == 0)
                return;

            reader.DataStream.Position = startOffset;

            while (reader.DataStream.Position < (startOffset + length))
            {
                var elementStore = ReadElement(reader.DataStream);
                if (!EqualityComparer<TElement>.Default.Equals(elementStore, default))
                    _store.Add(elementStore);
            }
        }

        public void WriteSegment<T, U>(T reader) where T : BinaryWriter, IWriter<U>
        {
            throw new NotImplementedException();
        }

        protected abstract TElement ReadElement(Stream dataStream);
        protected abstract void WriteElement(BinaryWriter writer, in TElement element);

        #region IList<TElement> implementation
        public TElement this[int index] {
            get => _store[index];
            set => _store[index] = value;
        }

        public int Count => _store.Count;
        public bool IsReadOnly => ((IList<TElement>)_store).IsReadOnly;

        public void Add(TElement item) => _store.Add(item);

        public void Clear() => _store.Clear();

        public bool Contains(TElement item) => _store.Contains(item);

        public void CopyTo(TElement[] array, int arrayIndex) => _store.CopyTo(array, arrayIndex);
    
        public IEnumerator<TElement> GetEnumerator() => _store.GetEnumerator();
    
        public int IndexOf(TElement item) => _store.IndexOf(item);

        public void Insert(int index, TElement item) => _store.Insert(index, item);
    
        public bool Remove(TElement item) => _store.Remove(item);
    
        public void RemoveAt(int index) => _store.RemoveAt(index);
    
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_store).GetEnumerator();
        }
        #endregion
    }
}
