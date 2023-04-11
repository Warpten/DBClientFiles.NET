using DBClientFiles.NET.Parsing.Runtime.Serialization;
using DBClientFiles.NET.Parsing.Versions;
using DBClientFiles.NET.Utils;
using DBClientFiles.NET.Utils.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations
{
    internal class CommonBlockHandler : ISegmentHandler
    {
        private bool _isWordAligned = true;
        private List<byte> _compressedData = new();

        private readonly StringBlockHandler _stringBlock;
        private readonly List<(int EntryCount, byte Type, Dictionary<int, int> OffsetMap)> _columnData = new ();

        private static readonly short[] CommonTypeSizes = new short[] {
            sizeof(int),   // String
            sizeof(short), // Short
            sizeof(byte),  // Byte
            sizeof(float), // Float
            sizeof(int)    // Int
        };
        private static readonly Predicate<Type>[] CommonTypes = new Predicate<Type>[]
        {
            t => t == typeof(string) || t == typeof(ReadOnlyMemory<byte>),
            t => t == typeof(short),
            t => t == typeof(byte),
            t => t == typeof(float),
            t => t == typeof(int)
        };

        public CommonBlockHandler(StringBlockHandler stringBlock)
        {
            _stringBlock = stringBlock;
        }

        #region ISegmentHandler
        public void ReadSegment(IBinaryStorageFile reader, long startOffset, long length)
        {
            // Note: a lot of this looks dumb in an (a probably futile) attempt to eliminate bounds check from
            // list access.

            reader.DataStream.Position = startOffset;
            Span<byte> rawData = new byte[length];
            reader.DataStream.Read(rawData);

            var numColumns = MemoryMarshal.Read<int>(rawData);
            var endOffset = startOffset + length;

            _isWordAligned = (length - sizeof(int) - (5 * numColumns)) % 8 == 0;

            var currentOffset = sizeof(int);
            for (var i = 0; i < numColumns; ++i)
            {
                var currentSlice = rawData[currentOffset..];
                var count = MemoryMarshal.Read<int>(currentSlice);
                currentOffset += sizeof(int);

                var type = MemoryMarshal.Read<byte>(currentSlice[currentOffset..]);
                currentOffset += sizeof(byte);

                var elementSize = _isWordAligned ? sizeof(int) : CommonTypeSizes[type];
                _compressedData.Capacity += CommonTypeSizes[type] * count; // Reserve additional space for this column

                var offsetMap = new Dictionary<int, int>();
                for (var j = 0; j < count; ++j)
                {
                    var recordID = MemoryMarshal.Read<int>(currentSlice[currentOffset..]);

                    // Get a slice of the bytes holding the value, with the padding removed
                    var valueSlice = currentSlice.Slice(currentOffset + sizeof(int), CommonTypeSizes[type]);

                    // Get a span over the chunk of `_compressedData` that holds the sequences of values
                    var compressedDataChunk = CollectionsMarshal.AsSpan(_compressedData).Slice(_compressedData.Count);

                    // Copy the slice and store the value offset
                    var compressedValueOffset = _compressedData.Count;

                    offsetMap.Add(recordID, compressedValueOffset);
                    valueSlice.CopyTo(compressedDataChunk);

                    // Advance the read cursor over the (possibly uncompressed) data
                    currentOffset += sizeof(int) + elementSize;
                }

                _columnData.Add((EntryCount: count, Type: type, OffsetMap: offsetMap));
            }
        }

        public void WriteSegment<T, U>(T reader) where T : BinaryWriter, IWriter<U>
        {
            throw new NotImplementedException();
        }
        #endregion

        public T Read<T>(int columnIndex, int recordID, Variant<int> defaultValue) where T : struct
        {
            Debug.Assert(columnIndex < _columnData.Count);

            var (entryCount, type, offsetMap) = CollectionsMarshal.AsSpan(_columnData)[columnIndex];
            Debug.Assert(CommonTypes[type](typeof(T)));

            if (!offsetMap.TryGetValue(recordID, out var dataOffset))
                return defaultValue.Cast<T>();

            return MemoryMarshal.Read<T>(CollectionsMarshal.AsSpan(_compressedData)[dataOffset..]);
        }

        public string ReadString(int columnIndex, int recordID, Variant<int> defaultValue)
            => _stringBlock.ReadString(Read<int>(columnIndex, recordID, defaultValue));

        public ReadOnlyMemory<byte> ReadUTF8(int columnIndex, int recordID, Variant<int> defaultValue)
            => _stringBlock.ReadUTF8(Read<int>(columnIndex, recordID, defaultValue));
    }
}
