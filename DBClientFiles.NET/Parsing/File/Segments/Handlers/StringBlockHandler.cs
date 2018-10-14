using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal sealed class StringBlockHandler : IBlockHandler, IDictionary<long, String>
    {
        private Dictionary<long, String> _block = new Dictionary<long, string>();

        private bool _internStrings;

        public StringBlockHandler(bool internStrings)
        {
            _internStrings = internStrings;
        }

        #region IFileBlock
        public BlockIdentifier Identifier { get; } = BlockIdentifier.StringBlock;

        public void Parse<T, U>(T reader, long startOffset, long length) where T : BinaryReader, IReader<U>
        {
            if (startOffset == 0 || length <= 2)
                return;

            reader.BaseStream.Seek(startOffset, SeekOrigin.Begin);

            // Not ideal but this will do
            var byteBuffer = new byte[length];
            int actualLength = reader.Read(byteBuffer, 0, (int)length);

            Debug.Assert(actualLength == length);

            int cursor = 0;

            while (cursor != length)
            {
                var stringStart = cursor;
                while (byteBuffer[cursor] != 0)
                    ++cursor;

                if (cursor - stringStart > 1)
                {
                    var value = Encoding.UTF8.GetString(byteBuffer, stringStart, cursor - stringStart);
                    if (_internStrings)
                        value = string.Intern(value);

                    _block[stringStart] = value;
                }

                cursor += 1;
            }
        }
        #endregion

        #region IDictionary<long, String>
        public string this[long key]
        {
            get => TryGetValue(key, out var value) ? value : string.Empty;
            set => _block[key] = value;
        }

        public ICollection<long> Keys => _block.Keys;
        public ICollection<string> Values => _block.Values;
        public int Count => _block.Count;
        public bool IsReadOnly => true;

        public void Add(long key, string value) => _block.Add(key, value);
        public void Add(KeyValuePair<long, string> item) => ((IDictionary<long, String>)_block).Add(item);

        public void Clear() => _block.Clear();

        public bool Contains(KeyValuePair<long, string> item) => ((IDictionary<long, String>)_block).Contains(item);
        public bool ContainsKey(long key) => _block.ContainsKey(key);

        public void CopyTo(KeyValuePair<long, string>[] array, int arrayIndex) => ((IDictionary<long, string>)_block).CopyTo(array, arrayIndex);

        public bool Remove(long key) => _block.Remove(key);
        public bool Remove(KeyValuePair<long, string> item) => ((IDictionary<long, string>)_block).Remove(item);

        public bool TryGetValue(long key, out string value) => _block.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<long, string>> GetEnumerator() => ((IDictionary<long, string>)_block).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<long, string>)_block).GetEnumerator();
        }
        #endregion
    }
}
