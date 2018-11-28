using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal sealed class IndexTableHandler<TKey> : IList<TKey>, IBlockHandler
    {
        private IList<TKey> _store = new List<TKey>();

        public BlockIdentifier Identifier { get; } = BlockIdentifier.IndexTable;

        public void ReadBlock<T>(T reader, long startOffset, long length) where T : BinaryReader, IParser
        {
            if (startOffset == 0 || length == 0)
                return;

            reader.BaseStream.Seek(startOffset, SeekOrigin.Begin);
            for (var i = 0; i < reader.Header.RecordCount; ++i)
            {
                // Do stuff
            }
        }

        public void WriteBlock<T, U>(T writer) where T : BinaryWriter, IWriter<U>
        {
        }

        public TKey this[int index] {
            get => _store[index];
            set => _store[index] = value;
        }

        public int Count => _store.Count;
        public bool IsReadOnly => _store.IsReadOnly;

        public void Add(TKey item) => _store.Add(item);
        public void Clear() => _store.Clear();
        public bool Contains(TKey item) => _store.Contains(item);
        public void CopyTo(TKey[] array, int arrayIndex) => _store.CopyTo(array, arrayIndex);

        public IEnumerator<TKey> GetEnumerator() => _store.GetEnumerator();

        public int IndexOf(TKey item) => _store.IndexOf(item);
        public void Insert(int index, TKey item) => _store.Insert(index, item);
        public bool Remove(TKey item) => _store.Remove(item);
        public void RemoveAt(int index) => _store.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => _store.GetEnumerator();
    }

}
