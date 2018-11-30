using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal sealed class IndexTableHandler : IList<int>, IBlockHandler
    {
        private IList<int> _store = new List<int>();

        public BlockIdentifier Identifier { get; } = BlockIdentifier.IndexTable;

        public void ReadBlock<T>(T reader, long startOffset, long length) where T : BinaryReader, IParser
        {
            if (startOffset == 0 || length == 0)
                return;

            reader.BaseStream.Seek(startOffset, SeekOrigin.Begin);
            _store = new List<int>((int)(length / sizeof(int)));
            for (var i = 0; i < reader.Header.RecordCount; ++i)
                _store[i] = reader.ReadInt32();
        }

        public void WriteBlock<T, U>(T writer) where T : BinaryWriter, IWriter<U>
        {
        }

        public int this[int index] {
            get => _store[index];
            set => _store[index] = value;
        }

        public int Count => _store.Count;
        public bool IsReadOnly => _store.IsReadOnly;

        public void Add(int item) => _store.Add(item);
        public void Clear() => _store.Clear();
        public bool Contains(int item) => _store.Contains(item);
        public void CopyTo(int[] array, int arrayIndex) => _store.CopyTo(array, arrayIndex);

        public IEnumerator<int> GetEnumerator() => _store.GetEnumerator();

        public int IndexOf(int item) => _store.IndexOf(item);
        public void Insert(int index, int item) => _store.Insert(index, item);
        public bool Remove(int item) => _store.Remove(item);
        public void RemoveAt(int index) => _store.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => _store.GetEnumerator();
    }

}
