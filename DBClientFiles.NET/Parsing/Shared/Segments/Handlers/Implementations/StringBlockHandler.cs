using DBClientFiles.NET.Parsing.Versions;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations
{
    internal sealed class StringBlockHandler : ISegmentHandler, IDictionary<long, String>
    {
        private Dictionary<long, string> _blockData = new Dictionary<long, string>();

        private bool _internStrings;

        public StringBlockHandler(bool internStrings)
        {
            _internStrings = internStrings;
        }

        #region IBlockHandler
        public SegmentIdentifier Identifier { get; } = SegmentIdentifier.StringBlock;

        public unsafe void ReadSegment(IBinaryStorageFile reader, long startOffset, long length)
        {
            if (length <= 2)
                return;

            reader.BaseStream.Seek(startOffset, SeekOrigin.Begin);

            // Not ideal but this will do
            var byteBuffer = ArrayPool<byte>.Shared.Rent((int) length);
            int actualLength = reader.BaseStream.Read(byteBuffer, 0, (int) length);

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

            ArrayPool<byte>.Shared.Return(byteBuffer);
        }

        public void WriteSegment<T, U>(T writer) where T : BinaryWriter, IWriter<U>
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
