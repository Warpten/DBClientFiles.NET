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
    internal sealed class StringBlockHandler : ISegmentHandler
    {
        private IMemoryOwner<byte> _blockData;
        private Dictionary<long /* offset */, long /* length */> _pointers = new(1024);

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

            _blockData = MemoryPool<byte>.Shared.Rent((int)length);

            reader.DataStream.Position = startOffset;
            reader.DataStream.Read(_blockData.Memory.Span);

            // Scan through the buffer, 4 bytes at a time, looking for null terminators
            var stringOffset = 1L;
            while (stringOffset < length)
            {
                Span<uint> wordBuffer = MemoryMarshal.Cast<byte, uint>(_blockData.Memory.Span[(int) stringOffset..]);

                var wordCursor = 0;
                var mask = wordBuffer[wordCursor];

                // Iterate until a zero byte is hit
                while (((mask - 0x01010101) & ~mask & 0x80808080) == 0)
                    mask = wordBuffer[++wordCursor];

                // Identify the exact zero byte
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

                // Compute actual string length and insert it
                var stringLength = wordCursor * sizeof(uint) + trailingCount;
                if (stringLength > 0)
                {
                    _pointers.Add(stringOffset, stringLength);

                    // Skip to the next string
                    stringOffset += stringLength + 1;
                }
                else
                    stringOffset += 1;
            }

        }

        public void WriteSegment<T, U>(T writer) where T : BinaryWriter, IWriter<U>
        {

        }
        #endregion

        public ReadOnlyMemory<byte> ReadUTF8(long offset)
        {
            if (_pointers.TryGetValue(offset, out var length))
                return _blockData.Memory.Slice((int) offset, (int) length);

            return Memory<byte>.Empty;
        }

        public string ReadString(long offset)
        {
            if (_pointers.TryGetValue(offset, out var length))
                return Encoding.UTF8.GetString(_blockData.Memory.Slice((int) offset, (int) length).Span);

            return string.Empty;
        }
    }
}
