using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader for the new common table (as seen in WDB6 file format).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    internal sealed class CommonTableReader<TKey> : SegmentReader
        where TKey : struct
    {
        private class DataBlock : IDisposable
        {
            private byte[] _dataBlock;
            private Dictionary<TKey, int> _valueOffsets;

            public DataBlock(FileReader reader, int segmentSize)
            {
                _dataBlock = reader.ReadBytes(segmentSize);
                Span<byte> blockSpan = _dataBlock;

                _valueOffsets = new Dictionary<TKey, int>(segmentSize / 8);
                for (var i = 0; i < _dataBlock.Length; i += 8)
                {
                    var key = MemoryMarshal.Read<TKey>(blockSpan.Slice(i, 8));
                    _valueOffsets[key] = i;
                }
            }

            public void Dispose()
            {
                _valueOffsets.Clear();
                _dataBlock = null;
            }

            public T ExtractValue<T>(TKey key, T defaultValue)
                where T : struct
            {
                if (!_valueOffsets.TryGetValue(key, out var offset))
                    return defaultValue;

                var auto = _dataBlock.AsSpan(offset);
                return MemoryMarshal.Read<T>(auto);
            }
        }

        private DataBlock[] _dataBlocks;

        public CommonTableReader(FileReader reader) : base(reader)
        {
        }

        protected override void Release()
        {
            for (var i = 0; i < _dataBlocks.Length; ++i)
                _dataBlocks[i].Dispose();
        }

        public void Initialize(IEnumerable<int> blockLengths)
        {
            if (Segment.Length == 0)
                return;

            FileReader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);

            var blocks = blockLengths.ToArray();
            _dataBlocks = new DataBlock[blocks.Length];
            for (var i = 0; i < blocks.Length; ++i)
                _dataBlocks[i] = new DataBlock(FileReader, blocks[i]);
        }

        public override void Read()
        {
        }

        public T ExtractValue<T>(int columnIndex, T defaultValue, TKey recordKey) where T : struct
        {
            if (_dataBlocks.Length <= columnIndex)
                return defaultValue;

            return _dataBlocks[columnIndex].ExtractValue(recordKey, defaultValue);
        }
    }
}
