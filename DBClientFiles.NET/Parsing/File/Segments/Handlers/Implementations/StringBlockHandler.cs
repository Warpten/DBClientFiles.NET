using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal sealed class StringBlockHandler : IBlockHandler, IDictionary<long, String>
    {
        private Dictionary<long, String> _blockData = new Dictionary<long, string>();

        private bool _internStrings;

        public StringBlockHandler(bool internStrings)
        {
            _internStrings = internStrings;
        }

        #region IBlockHandler
        public BlockIdentifier Identifier { get; } = BlockIdentifier.StringBlock;

        public unsafe void ReadBlock(IBinaryStorageFile reader, long startOffset, long length)
        {
            if (length <= 2)
                return;

            reader.BaseStream.Seek(startOffset, SeekOrigin.Begin);

            // Not ideal but this will do
            var byteBuffer = new byte[length];
            int actualLength = reader.BaseStream.Read(byteBuffer, 0, (int)length);

#if __DEBUG

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

                    _blockData[stringStart] = value;
                }

                cursor += 1;
            }
#else
            var intBuffer = (int*)Unsafe.AsPointer(ref byteBuffer[0]);
            int cursor = 0;
            int bufferSize = actualLength / 4;

            var builder = new StringBuilder();
            var startOffsetStr = 0;
            while (cursor != bufferSize)
            {
                int mask = intBuffer[cursor];
                while (mask != 0)
                {
                    var maskAsBytes = (byte*) Unsafe.AsPointer(ref mask);
                    for (var i = 0; i < 4; ++i)
                    {
                        if (maskAsBytes[i] == 0)
                        {
                            var value = builder.ToString();
                            if (_internStrings)
                                value = string.Intern(value);

                            _blockData[startOffsetStr] = value;
                            startOffsetStr = cursor + i + 1;
                            builder.Clear();
                        }
                        else
                            builder.Append((char) maskAsBytes[i]);
                    }

                    ++cursor;
                    if (cursor == bufferSize)
                        break;
                    
                    mask = intBuffer[cursor];
                }
            }

            for (var i = cursor * 4; i < actualLength - 1; ++i)
                builder.Append((char) byteBuffer[i]);

            _blockData[startOffsetStr] = builder.ToString();
#endif
        }

        public void WriteBlock<T, U>(T writer) where T : BinaryWriter, IWriter<U>
        {

        }
#endregion

        #region IDictionary<long, String>
        public string this[long key]
        {
            get => TryGetValue(key, out var value) ? value : string.Empty;
            set => _blockData[key] = value;
        }

        public ICollection<long> Keys => _blockData.Keys;
        public ICollection<string> Values => _blockData.Values;
        public int Count => _blockData.Count;
        public bool IsReadOnly => true;

        public void Add(long key, string value) => _blockData.Add(key, value);
        public void Add(KeyValuePair<long, string> item) => ((IDictionary<long, String>)_blockData).Add(item);

        public void Clear() => _blockData.Clear();

        public bool Contains(KeyValuePair<long, string> item) => ((IDictionary<long, String>)_blockData).Contains(item);
        public bool ContainsKey(long key) => _blockData.ContainsKey(key);

        public void CopyTo(KeyValuePair<long, string>[] array, int arrayIndex) => ((IDictionary<long, string>)_blockData).CopyTo(array, arrayIndex);

        public bool Remove(long key) => _blockData.Remove(key);
        public bool Remove(KeyValuePair<long, string> item) => ((IDictionary<long, string>)_blockData).Remove(item);

        public bool TryGetValue(long key, out string value) => _blockData.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<long, string>> GetEnumerator() => ((IDictionary<long, string>)_blockData).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<long, string>)_blockData).GetEnumerator();
        }
        #endregion
    }
}
