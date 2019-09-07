using DBClientFiles.NET.Parsing.Versions;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations
{
    internal sealed class StringBlockHandler : ISegmentHandler, IDictionary<long, string>
    {
        private readonly Dictionary<long, string> _blockData = new Dictionary<long, string>(5000);

        #region IBlockHandler
        public void ReadSegment(IBinaryStorageFile reader, long startOffset, long length)
        {
            // Impossible for length to be lower than 2
            // Consider this: length 2 has to be 00 ??
            // But strings have to be null terminated thus data should be 00 ?? 00
            // Which is 3 bytes.
            // Above sligtly wrong, nothing prevents a 1-byte string block with 00 but in that case we already handle it
            // by returning string.Empty if offset was not found in the block.
            // This is based off the assumption that strings at offset 0 will always be the null string
            // which it has to be for empty string blocks. For non-empty blocks,  let's just say blizzard's space saving
            // track record isn't the best, and saving one byte by pointing the null string to the middle of any delimiter
            // is something probably not worth the effort.
            if (length <= 2)
                return;

            reader.DataStream.Position = startOffset;

            // Next target: Pray for C# 9 to expose null-terminated strings
#if EXPERIMENTAL // This nets about 4% gain on string parsing alone (which is hardly a bottleneck, but still)
            var internStrings = reader.Options.InternStrings;

            // Requesting bytes aligned to int boundary
            // Add an extra byte to ensure we never go out of bounds
            var alignedLength = ((int) length + sizeof(int) - 1) & ~(sizeof(int) - 1);
            var byteBuffer = ArrayPool<byte>.Shared.Rent(alignedLength);
            Span<byte> byteSpan = new Span<byte>(byteBuffer, 0, alignedLength);

            // Read exactly what is needed
            reader.DataStream.Read(byteSpan.Slice(0, (int) length));
            // Zero the trailing bytes because Rent does not guarantee that
            // One is enough to ensure mask checking will fail
            byteBuffer[length] = 0x00;

            var stringOffset = 1L;
            while (stringOffset < length)
            {
                var wordSpan = MemoryMarshal.Cast<byte, uint>(byteSpan.Slice((int) stringOffset));

                var wordCursor = 0;
                var mask = wordSpan[wordCursor];
                while (((mask - 0x01010101) & ~mask & 0x80808080) == 0)
                    mask = wordSpan[++wordCursor];

                var trailingCount = 0;
                if ((mask & 0x000000FF) != 0x00)
                {
                    ++trailingCount;
                    if ((mask & 0x0000FF00) != 0x00)
                    {
                        ++trailingCount;
                        if ((mask & 0x00FF0000) != 0x00)
                            ++trailingCount;
                    }
                }

                var strLength = (wordCursor * sizeof(int) + trailingCount);
                if (strLength > 0)
                {
                    var value = (reader.Options.Encoding ?? Encoding.UTF8).GetString(byteBuffer, (int) stringOffset, strLength);
                    if (internStrings)
                        value = string.Intern(value);

                    _blockData.Add(stringOffset, value);
                    stringOffset += strLength + 1;
                }
                else
                {
                    ++stringOffset;
                }
            }

            ArrayPool<byte>.Shared.Return(byteBuffer);

#else
            var byteBuffer = ArrayPool<byte>.Shared.Rent((int) length);
            var actualLength = reader.DataStream.Read(byteBuffer, 0, (int) length);

            Debug.Assert(actualLength == length);

            // We start at 1 because 0 is always 00, aka null string
            var cursor = 1;
            while (cursor != length)
            {
                var stringStart = cursor;
                while (byteBuffer[cursor] != 0)
                    ++cursor;

                if (cursor - stringStart > 1)
                {
                    var value = (reader.Options.Encoding ?? Encoding.UTF8).GetString(byteBuffer, stringStart, cursor - stringStart);
                    if (reader.Options.InternStrings)
                        value = string.Intern(value);

                    _blockData[stringStart] = value;
                }

                cursor += 1;
            }

            ArrayPool<byte>.Shared.Return(byteBuffer);
#endif
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
        public void Add(KeyValuePair<long, string> item) => ((IDictionary<long, string>)_blockData).Add(item);

        public void Clear() => _blockData.Clear();

        public bool Contains(KeyValuePair<long, string> item) => ((IDictionary<long, string>)_blockData).Contains(item);
        public bool ContainsKey(long key) => _blockData.ContainsKey(key);

        public void CopyTo(KeyValuePair<long, string>[] array, int arrayIndex) => ((IDictionary<long, string>)_blockData).CopyTo(array, arrayIndex);

        public bool Remove(long key) => _blockData.Remove(key);
        public bool Remove(KeyValuePair<long, string> item) => ((IDictionary<long, string>)_blockData).Remove(item);

        public bool TryGetValue(long key, out string value) => _blockData.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<long, string>> GetEnumerator() => _blockData.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_blockData).GetEnumerator();
#endregion
    }
}
