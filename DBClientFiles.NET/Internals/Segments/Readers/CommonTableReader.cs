using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader for legacy common table (as seen in WDB6 file format).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    internal sealed class CommonTableReader<TKey> : SegmentReader
        where TKey : struct
    {
        private Memory<byte>[] _dataBlocks;
        private int[] _dataSizes;

        public CommonTableReader(FileReader reader) : base(reader)
        {
        }

        protected override void Release()
        {
        }

        public void Initialize(IEnumerable<int> blockLengths)
        {
            _dataSizes = blockLengths.ToArray();
            _dataBlocks = new Memory<byte>[_dataSizes.Length];
        }

        public override void Read()
        {
            if (Segment.Length == 0)
                return;

            FileReader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            for (var i = 0; i < _dataSizes.Length; ++i)
                _dataBlocks[i] = FileReader.ReadBytes(_dataSizes[i]);
        }

        public T ExtractValue<T>(int columnIndex, T defaultValue, TKey recordKey) where T : struct
        {
            // TODO: This is horribly slow.
            var slice = _dataBlocks[columnIndex];
            var nodesSlice = MemoryMarshal.Cast<byte, Node<T>>(slice.Span);
            
            for (var i = 0; i < nodesSlice.Length; ++i)
                if (nodesSlice[i].Key.Equals(recordKey))
                    return nodesSlice[i].Value;

            return defaultValue;
        }
        
        private struct Node<T> where T : struct
        {
#pragma warning disable 649
            public TKey Key;
            public T Value;
#pragma warning restore 649

            public override string ToString()
            {
                return $"[{Key}] = {Value}";
            }
        }
    }
}
