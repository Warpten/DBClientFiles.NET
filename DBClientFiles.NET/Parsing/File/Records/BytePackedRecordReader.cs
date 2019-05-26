using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Parsing.File.Records
{
    /// <summary>
    /// An implementation of <see cref="IRecordReader"/> tailored for WDB5 and WDB6. The values it reads are always
    /// aligned to byte boundaries.
    /// </summary>
    internal sealed unsafe class ByteAlignedRecordReader : IRecordReader
    {
        private byte[] _stagingBuffer;

        private readonly StringBlockHandler _stringBlock;

        public ByteAlignedRecordReader(IBinaryStorageFile fileReader, int recordSize)
        {
            _stringBlock = fileReader.FindBlock(BlockIdentifier.StringBlock)?.Handler as StringBlockHandler;

            _stagingBuffer = new byte[recordSize + 8]; // Allocating 8 extra bytes for packed reads to make sure we don't start reading another process's memory out of bad luck
        }

        public void Dispose()
        {
        }

        public void LoadStream(Stream dataStream, int recordSize)
        {
            Debug.Assert(recordSize + 8 <= _stagingBuffer.Length, "The buffer of ByteAlignedRecordReader is expected to be able to contain every record");

            dataStream.Read(_stagingBuffer, 0, recordSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public string ReadString()
        {
            throw new NotImplementedException();
        }

        public T ReadImmediate<T>(int bitOffset, int bitCount) where T : unmanaged
        {
            // Values here should always be aligned
            Debug.Assert((bitCount & 7) == 0, "WDB5 and WDB6 values should always be aligned to 8-byte boundaries!");

            var byteOffset = bitOffset / 8;
            var baseShift = 64 - bitCount;

            var value = (*(ulong*) Unsafe.AsPointer(ref _stagingBuffer[byteOffset]) << (baseShift - (bitOffset & 7))) >> baseShift;
            return *(T*)&value;
        }

        public string ReadString(int bitOffset, int bitCount)
        {
            throw new NotImplementedException();
            /*
            // Values here should always be aligned
            Debug.Assert((bitCount & 7) == 0, "WDB5 and WDB6 values should always be aligned to 8-byte boundaries!");

            if (_stringBlock == null)
            {
                var startCursor = (sbyte*)Unsafe.AsPointer(ref _stagingBuffer[_byteCursor]);
                var endCursor = startCursor;
                while (*endCursor != 0)
                    ++endCursor;

                _byteCursor += (int)(endCursor - startCursor);

                return new string(startCursor, 0, (int)(endCursor - startCursor));
            }

            return _stringBlock[ReadImmediate<uint>(bitOffset, bitCount)];*/
        }

    }
}
