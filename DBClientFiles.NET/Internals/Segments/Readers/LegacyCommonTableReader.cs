using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Internals.Versions;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader for legacy common table (as seen in WDB6 file format).
    /// </summary>
    /// <typeparam name="TValue">The record type of the currently operated file.</typeparam>
    internal sealed class LegacyCommonTableReader<TKey, TValue> : ISegmentReader<TValue>
        where TValue : class, new()
    {
        public Segment<TValue> Segment { get; set; }
        public FileReader Storage => Segment.Storage;

        private bool _isPadded = true;
        private Type[] _memberTypes;

        private Dictionary<TKey, byte[]>[] _valueOffsets;
        
        public LegacyCommonTableReader()
        {
            if (Segment.Length == 0)
                return;

            Storage.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            Parse();
        }

        private void Parse()
        {
            var columnCount = Storage.ReadInt32();

            _memberTypes = new Type[columnCount];
            _valueOffsets = new Dictionary<TKey, byte[]>[columnCount];
            for (var i = 0; i < columnCount; ++i)
                _valueOffsets[i] = new Dictionary<TKey, byte[]>();

            /// Try to read everything as unpacked.
            /// If reading fails (probably because the cursor is now beyond the end of the file), then the structure is packed.

            bool isDryRun = true;
            bool successfulParse = true;
            for (var i = 0; i < columnCount && successfulParse; ++i)
                successfulParse |= AssertReadColumn(i, isDryRun, false);

            _isPadded = successfulParse;
            if (successfulParse)
            {
                // Structure is actually packed.
                Storage.BaseStream.Seek(Segment.StartOffset + 4, SeekOrigin.Begin);

                for (var i = 0; i < columnCount && successfulParse; ++i)
                    successfulParse |= AssertReadColumn(i, false, true);
            }
            else
            {
                // Structure is unpacked.
                // Read again, but it's not a dry run this time.
                Storage.BaseStream.Seek(Segment.StartOffset + 4, SeekOrigin.Begin);

                for (var i = 0; i < columnCount; ++i)
                    AssertReadColumn(i, true, false);
            }

            // We also need to check here that we properly went through the entirety of the block - for safekeeping.
            if (Storage.BaseStream.Position != Segment.EndOffset)
                throw new FileLoadException();
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
            var entryCount = Storage.ReadInt32();
            var entryType = Storage.ReadByte();

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
                var newPosition = Storage.BaseStream.Seek((4 + dataSize) * entryCount, SeekOrigin.Current);
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
                    _valueOffsets[columnIndex][Storage.ReadStruct<TKey>()] = Storage.ReadBytes(dataSize);
                    var newPosition = Storage.BaseStream.Seek(keySize + dataSize, SeekOrigin.Current);

                    // And same as the comment above here.
                    if (newPosition > Segment.EndOffset)
                        return false;
                }
                return true;
            }
        }

        public void Dispose()
        {
            Segment = null;
        }

        public void Read()
        {
            
        }

        public unsafe T ExtractValue<T>(int columnIndex, TKey recordKey) where T : struct
        {
            var dict = _valueOffsets[columnIndex];
            if (!dict.TryGetValue(recordKey, out var dataBlock))
                return default;

            fixed (byte* buffer = dataBlock)
                return FastStructure<T>.PtrToStructure(new IntPtr(buffer));
        }
    }
}
