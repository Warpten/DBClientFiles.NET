using DBClientFiles.NET.Parsing.Versions;
using System;
using System.IO;
using DBClientFiles.NET.Utils.Extensions;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations
{
    internal sealed class OffsetMapHandler : ISegmentHandler
    {
        private Memory<(int Offset, short Size)> _store;

        public void ReadSegment(IBinaryStorageFile reader, long startOffset, long length)
        {
            if (startOffset == 0 || length == 0)
                return;

            reader.DataStream.Position = startOffset;

            var i = 0;
            Count = (int)(length / (sizeof(int) + sizeof(short)));

            _store = new Memory<(int, short)>(new (int, short)[Count]);

            while (reader.DataStream.Position < (startOffset + length))
            {
                var key = reader.DataStream.Read<int>();
                var value = reader.DataStream.Read<short>();

                if (key == 0 || value == 0)
                {
                    --Count;
                    continue;
                }

                _store.Span[i++] = (key, value);
            }

            _store = _store.Slice(0, Count);
        }

        public void WriteSegment<T, U>(T writer) where T : BinaryWriter, IWriter<U>
        {
        }

        public int Count { get; private set; }

        public (int Offset, int Length) this[int index] => _store.Span[index];
    }
}
