using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace DBClientFiles.NET.Parsing.File.Records
{
    /// <summary>
    /// An implementation of <see cref="IRecordReader"/> tailored for WDBC and WDB2. The records
    /// within these files always have the same sizes. It is only able of performing aligned reads.
    /// </summary>
    internal sealed unsafe class PackedRecordReader : IRecordReader
    {
        private byte[] _stagingBuffer;

        private int _byteCursor;
        private readonly StringBlockHandler _stringBlock;

        public PackedRecordReader(IBinaryStorageFile fileReader, int recordSize)
        {
            _stringBlock = fileReader.FindBlockHandler<StringBlockHandler>(BlockIdentifier.StringBlock);

            _stagingBuffer = new byte[recordSize + 8];
            _byteCursor = 0;
        }

        public void Dispose()
        {
        }

        public void LoadStream(Stream dataStream, int recordSize)
        {
            // This will only ever trigger on offset maps
            if (recordSize > _stagingBuffer.Length)
                Array.Resize(ref _stagingBuffer, recordSize);

            dataStream.Read(_stagingBuffer, 0, recordSize);
            _byteCursor = 0;
        }

        public void LoadStream(Stream dataStream)
        {
            dataStream.Read(_stagingBuffer, 0, _stagingBuffer.Length);
            _byteCursor = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            var value = *(T*)Unsafe.AsPointer(ref _stagingBuffer[_byteCursor]);
            _byteCursor += Unsafe.SizeOf<T>();
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ReadArray<T>(int count) where T : unmanaged
        {
            // Console.WriteLine("Reading " + count + " elements of size " + sizeof(T) + " bytes each at offset " + _byteCursor);

            var rentedBuffer = new T[count];

            Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref rentedBuffer[0]),
                Unsafe.AsPointer(ref _stagingBuffer[_byteCursor]), (uint)(count * Unsafe.SizeOf<T>()));
            _byteCursor += count * Unsafe.SizeOf<T>();

            // for (var i = 0; i < count; ++i)
            //     rentedBuffer[i] = Read<T>();

            // _byteCursor += count * Unsafe.SizeOf<T>();
            return rentedBuffer;
        }

        public string ReadString()
        {
            if (_stringBlock == null)
            {
                var startCursor = (sbyte*)Unsafe.AsPointer(ref _stagingBuffer[_byteCursor]);
                var endCursor = startCursor;
                while (*endCursor != 0)
                    ++endCursor;

                _byteCursor += (int)(endCursor - startCursor);

                return new string(startCursor, 0, (int)(endCursor - startCursor));
            }

            return _stringBlock[Read<uint>()];
        }

        public string[] ReadStringArray(int count)
        {
            var value = new string[count];
            if (_stringBlock == null)
            {
                for (var i = 0; i < count; ++i)
                    value[i] = ReadString();
            }
            else
            {
                var stringOffsets = ReadArray<uint>(count);
                for (var i = 0; i < count; ++i)
                    value[i] = _stringBlock[stringOffsets[i]];
            }

            return value;
        }

        public T Read<T>(int bitCount) where T : unmanaged
        {
            // Values here should always be aligned
            Debug.Assert((bitCount & 7) != 0, "WDB5 and WDB6 values should always be aligned to 8-byte boundaries!");

            var value = *(long*)Unsafe.AsPointer(ref _stagingBuffer[_byteCursor]);
            value = (value >> (64 - bitCount));

            _byteCursor += bitCount / 8;

            return *(T*)Unsafe.AsPointer(ref value);
        }

        public T[] ReadArray<T>(int count, int elementBitCount) where T : unmanaged
        {
            // Values here should always be aligned
            Debug.Assert((elementBitCount & 7) != 0, "WDB5 and WDB6 values should always be aligned to 8-byte boundaries!");

            var values = new T[count];
            for (var i = 0; i < count; ++i)
                values[i] = Read<T>(elementBitCount);

            return values;
        }

        public string ReadString(int bitCount)
        {
            // Values here should always be aligned
            Debug.Assert((bitCount & 7) != 0, "WDB5 and WDB6 values should always be aligned to 8-byte boundaries!");

            if (_stringBlock == null)
            {
                var startCursor = (sbyte*)Unsafe.AsPointer(ref _stagingBuffer[_byteCursor]);
                var endCursor = startCursor;
                while (*endCursor != 0)
                    ++endCursor;

                _byteCursor += (int)(endCursor - startCursor);

                return new string(startCursor, 0, (int)(endCursor - startCursor));
            }

            return _stringBlock[Read<uint>(bitCount)];
        }

        public string[] ReadStringArray(int count, int elementBitCount)
        {
            // Values here should always be aligned
            Debug.Assert((elementBitCount & 7) != 0, "WDB5 and WDB6 values should always be aligned to 8-byte boundaries!");

            var value = new string[count];
            if (_stringBlock == null)
            {
                for (var i = 0; i < count; ++i)
                    value[i] = ReadString(elementBitCount);
            }
            else
            {
                var stringOffsets = ReadArray<uint>(count, elementBitCount);
                for (var i = 0; i < count; ++i)
                    value[i] = _stringBlock[stringOffsets[i]];
            }

            return value;
        }
    }
}
