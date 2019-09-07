using DBClientFiles.NET.Parsing.Versions;
using System;
using System.IO;
using System.Text;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations
{
    internal sealed class OffsetMapHandler : ISegmentHandler
    {
        private Memory<(int Offset, short Size)> _store;

        public SegmentIdentifier Identifier { get; } = SegmentIdentifier.CopyTable;

        public void ReadSegment(IBinaryStorageFile reader, long startOffset, long length)
        {
            if (startOffset == 0 || length == 0)
                return;

            reader.DataStream.Position = startOffset;

            int i = 0;
            Count = (int)(length / (sizeof(int) + sizeof(short)));

            _store = new Memory<(int, short)>(new (int, short)[Count]);

            using (var streamReader = new BinaryReader(reader.DataStream, Encoding.UTF8, true))
            {
                while (reader.DataStream.Position < (startOffset + length))
                {
                    var key = streamReader.ReadInt32();
                    var value = streamReader.ReadInt16();

                    if (key == 0 || value == 0)
                    {
                        --Count;
                        continue;
                    }

                    _store.Span[i++] = (key, value);
                }
            }

            _store = _store.Slice(0, Count);
        }

        public void WriteSegment<T, U>(T writer) where T : BinaryWriter, IWriter<U>
        {
        }

        public int Count { get; private set; }

        public (int Offset, int Length) this[int index] => _store.Span[index];

        public int GetLargestRecordSize()
        {
            var size = 0;
            for (var i = 0; i < _store.Length / 6; ++i)
            {
                int spanSize = _store.Span[i].Size;
                if (size < spanSize)
                    size = spanSize;
            }

            return size;
        }
    }
}
