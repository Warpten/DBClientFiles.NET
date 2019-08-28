using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Parsing.Shared.Records
{
    /// <summary>
    /// An implementation of <see cref="IRecordReader"/> tailored for WDBC and WDB2. The records
    /// within these files always have the same sizes. It is only able of performing aligned sequential reads.
    /// </summary>
    internal sealed unsafe class AlignedRecordReader : IRecordReader
    {
        private byte[] _stagingBuffer;

        private int _byteCursor;
        private readonly StringBlockHandler _stringBlock;

        public AlignedRecordReader(IBinaryStorageFile fileReader, int bufferSize)
        {
            _stringBlock = fileReader.FindSegment(SegmentIdentifier.StringBlock)?.Handler as StringBlockHandler;

            _stagingBuffer = ArrayPool<byte>.Shared.Rent(bufferSize + 8);
            _byteCursor = 0;
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(_stagingBuffer);
        }

        public void LoadStream(Stream dataStream, int recordSize)
        {
            Debug.Assert(recordSize == _stagingBuffer.Length - 8, "AlignedRecordReader expects all the records to have the same size.");

            dataStream.Read(_stagingBuffer, 0, recordSize);
            _byteCursor = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            var value = *(T*) Unsafe.AsPointer(ref _stagingBuffer[_byteCursor]);
            _byteCursor += Unsafe.SizeOf<T>();
            return value;
        }
        
        public string ReadString()
        {
            if (_stringBlock == null)
            {
                var startCursor = (sbyte*) Unsafe.AsPointer(ref _stagingBuffer[_byteCursor]);
                var endCursor = startCursor;
                while (*endCursor != 0)
                    ++endCursor;

                return new string(startCursor, 0, (int) (endCursor - startCursor));
            }

            return _stringBlock[Read<uint>()];
        }
        
        public T ReadImmediate<T>(int bitoffset, int bitCount) where T : unmanaged
        {
            throw new NotSupportedException();
        }
        
        public string ReadString(int bitOffset, int bitCount)
        {
            throw new NotSupportedException();
        }
    }
}
