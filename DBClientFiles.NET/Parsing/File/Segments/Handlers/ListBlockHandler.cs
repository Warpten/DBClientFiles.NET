using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal abstract class ListBlockHandler<TElement> : IBlockHandler, IList<TElement>
    {
        private List<TElement> _store;
        
        protected ListBlockHandler()
        {
            _store = new List<TElement>();
        }

        protected ListBlockHandler(List<TElement> store)
        {
            _store = store;
        }

        public void ReadBlock(IBinaryStorageFile reader, long startOffset, long length)
        {
            if (length == 0)
                return;

            Debug.Assert(reader.BaseStream.Position == startOffset, "Out-of-place parsing!");

            using (var streamReader = new BinaryReader(reader.BaseStream, Encoding.UTF8, true))
                while (reader.BaseStream.Position < (startOffset + length))
                    _store.Add(ReadElement(streamReader));
        }

        public void WriteBlock<T, U>(T reader) where T : BinaryWriter, IWriter<U>
        {
            throw new NotImplementedException();
        }

        protected abstract TElement ReadElement(BinaryReader reader);
        protected abstract void WriteElement(BinaryWriter writer, in TElement element);

        #region IList<TElement> implementation
        public TElement this[int index] {
            get => ((IList<TElement>)_store)[index];
            set => ((IList<TElement>)_store)[index] = value;
        }

        public int Count => ((IList<TElement>)_store).Count;
        public bool IsReadOnly => ((IList<TElement>)_store).IsReadOnly;

        public void Add(TElement item)
        {
            ((IList<TElement>)_store).Add(item);
        }

        public void Clear()
        {
            ((IList<TElement>)_store).Clear();
        }

        public bool Contains(TElement item)
        {
            return ((IList<TElement>)_store).Contains(item);
        }

        public void CopyTo(TElement[] array, int arrayIndex)
        {
            ((IList<TElement>)_store).CopyTo(array, arrayIndex);
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return ((IList<TElement>)_store).GetEnumerator();
        }

        public int IndexOf(TElement item)
        {
            return ((IList<TElement>)_store).IndexOf(item);
        }

        public void Insert(int index, TElement item)
        {
            ((IList<TElement>)_store).Insert(index, item);
        }

        public bool Remove(TElement item)
        {
            return ((IList<TElement>)_store).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<TElement>)_store).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<TElement>)_store).GetEnumerator();
        }
        #endregion
    }
}
