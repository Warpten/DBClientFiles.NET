using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader for legacy common table (as seen in WDB6 file format).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue">The record type of the currently operated file.</typeparam>
    internal sealed class CommonTableReader<TKey> : SegmentReader
        where TKey : struct
    {
        private Type[] _memberTypes;

        private Dictionary<TKey, byte[]>[] _valueOffsets;

        public CommonTableReader(FileReader reader) : base(reader)
        {
        }

        protected override void Release()
        {
            _valueOffsets = null;
        }

        public override void Read()
        {
            if (Segment.Length == 0)
                return;

            FileReader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            var columnCount = FileReader.ReadInt32();

            _memberTypes = new Type[columnCount];
            _valueOffsets = new Dictionary<TKey, byte[]>[columnCount];
            for (var i = 0; i < columnCount; ++i)
                _valueOffsets[i] = new Dictionary<TKey, byte[]>();

            for (var i = 0; i < columnCount; ++i)
                AssertReadColumn(i, false, true);
        }

        /// <summary>
        /// Reads a column, also asserting if the common block is packed or not.
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="dryRun"></param>
        /// <param name="readAsPadded"></param>
        /// <returns>true if the block read correctly, false otherwise.</returns>
        private bool AssertReadColumn(int columnIndex, bool dryRun, bool readAsPadded)
        {
            var entryCount = FileReader.ReadInt32();
            var entryType = FileReader.ReadByte();

            var dataSize = 4;
            switch (entryType)
            {
                case 0: // string
                    _memberTypes[columnIndex] = typeof(string);
                    break;
                case 3: // float
                    _memberTypes[columnIndex] = typeof(float);
                    break;
                case 4: // int
                    _memberTypes[columnIndex] = typeof(int);
                    break;
                case 1: // short
                    _memberTypes[columnIndex] = typeof(short);
                    if (!readAsPadded)
                        dataSize = 2;
                    break;
                case 2: // byte
                    _memberTypes[columnIndex] = typeof(byte);
                    if (!readAsPadded)
                        dataSize = 1;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (dryRun)
            {
                var newPosition = FileReader.BaseStream.Seek((4 + dataSize) * entryCount, SeekOrigin.Current);
                // Hopefully when this happens it means we were trying to read as non-packed.
                if (newPosition > Segment.EndOffset)
                    return false;
                return true;
            }
            else
            {
                var keySize = Math.Min(4, SizeCache<TKey>.Size);

                for (var i = 0; i < entryCount; ++i)
                {
                    _valueOffsets[columnIndex][FileReader.ReadStruct<TKey>()] = FileReader.ReadBytes(dataSize);
                    var newPosition = FileReader.BaseStream.Seek(keySize + dataSize, SeekOrigin.Current);

                    // And same as the comment above here.
                    if (newPosition > Segment.EndOffset)
                        return false;
                }
                return true;
            }
        }

        public unsafe T ExtractValue<T>(int columnIndex, TKey recordKey) where T : struct
        {
            var dict = _valueOffsets[columnIndex];
            if (!dict.TryGetValue(recordKey, out var dataBlock))
                return default;

            fixed (byte* buffer = dataBlock)
                return FastStructure.PtrToStructure<T>(new IntPtr(buffer));
        }
    }
}
