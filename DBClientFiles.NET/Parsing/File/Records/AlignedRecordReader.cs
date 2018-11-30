using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Parsing.File.Records
{
    /// <summary>
    /// An implementation of <see cref="IRecordReader"/> tailored for WDBC and WDB2. The records
    /// within these files always have the same sizes. It is only able of performing aligned reads.
    /// </summary>
    internal sealed unsafe class AlignedRecordReader : IRecordReader
    {
        private byte[] _stagingBuffer;

        private int _byteCursor;
        private readonly StringBlockHandler _stringBlock;

        public AlignedRecordReader(IBinaryStorageFile fileReader, int recordSize)
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
            var value = *(T*) Unsafe.AsPointer(ref _stagingBuffer[_byteCursor]);
            _byteCursor += Unsafe.SizeOf<T>();
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ReadArray<T>(int count) where T : unmanaged
        {
            //Console.WriteLine("Reading " + count + " elements of size " + sizeof(T) + " bytes each at offset " + _byteCursor);

            var rentedBuffer = new T[count];

            Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref rentedBuffer[0]), Unsafe.AsPointer(ref _stagingBuffer[_byteCursor]), (uint)(count * Unsafe.SizeOf<T>()));
            _byteCursor += count * Unsafe.SizeOf<T>();

            // for (var i = 0; i < count; ++i)
            //     rentedBuffer[i] = Read<T>();

            // _byteCursor += count * Unsafe.SizeOf<T>();
            // Unsafe.CopyBlock(Unsafe.AsPointer(ref rentedBuffer[0]), dataPtr, (uint)(count * Unsafe.SizeOf<T>()));
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

                return new string(startCursor, 0, (int) (endCursor - startCursor));
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
            throw new InvalidOperationException();
        }

        public T[] ReadArray<T>(int count, int elementBitCount) where T : unmanaged
        {
            throw new InvalidOperationException();
        }

        public string ReadString(int bitCount)
        {
            throw new InvalidOperationException();
        }

        public string[] ReadStringArray(int count, int elementBitCount)
        {
            throw new InvalidOperationException();
        }
    }
}
