using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DBClientFiles.NET.Parsing.Shared.Records
{
    /// <summary>
    /// An implementation of <see cref="IRecordReader"/> tailored for WDB5 and WDB6. The values it reads are always
    /// aligned to byte boundaries.
    /// </summary>
    internal abstract class ByteAlignedRecordReader : IDisposable
    {
        public static class Methods
        {
            public static readonly MethodInfo Read = typeof(ByteAlignedRecordReader).GetMethod("Read", new[] { typeof(int), typeof(int) });
            public static readonly MethodInfo ReadString = typeof(ByteAlignedRecordReader).GetMethod("ReadString", new[] { typeof(int), typeof(int) });
            public static readonly MethodInfo ReadUTF8 = typeof(ByteAlignedRecordReader).GetMethod("ReadUTF8", new[] { typeof(int), typeof(int) });
        }

        private byte[] _stagingBuffer;

        protected ByteAlignedRecordReader()
        {
        }

        protected ByteAlignedRecordReader(int recordSize)
        {
            _stagingBuffer = ArrayPool<byte>.Shared.Rent(recordSize + 8);
        }

        public void Dispose()
        {
            if (_stagingBuffer != null)
                ArrayPool<byte>.Shared.Return(_stagingBuffer);
        }

        public void LoadStream(Stream dataStream, int recordSize)
        {
            // Realloc' up if need be
            if (_stagingBuffer != null && _stagingBuffer.Length < recordSize)
            {
                ArrayPool<byte>.Shared.Return(_stagingBuffer);
                _stagingBuffer = ArrayPool<byte>.Shared.Rent(recordSize + 8);
            }

            dataStream.Read(_stagingBuffer, 0, recordSize);
        }

        public T Read<T>(int bitOffset, int bitCount) where T : unmanaged
        {
            // Values here should always be aligned
            Debug.Assert((bitCount & 7) == 0, "WDB5 and WDB6 values should always be aligned to 8-bit boundary!");

            var longValue = Unsafe.As<byte, ulong>(ref _stagingBuffer[bitOffset / 8]);
            longValue = (longValue << (64 - bitCount - (bitOffset % 8))) >> (64 - bitCount);
            return Unsafe.As<ulong, T>(ref longValue);
        }

        public abstract string ReadString(int bitOffset, int bitCount);
        public abstract ReadOnlyMemory<byte> ReadUTF8(int bitOffset, int bitCount);

        public sealed class WithStringBlock : ByteAlignedRecordReader
        {
            private readonly StringBlockHandler _stringBlock;

            public WithStringBlock(StringBlockHandler stringBlock, int recordSize) : base(recordSize)
                => _stringBlock = stringBlock;
        
            public override string ReadString(int bitOffset, int bitCount)
                => _stringBlock.ReadString(Read<int>(bitOffset, bitCount));

            public override ReadOnlyMemory<byte> ReadUTF8(int bitOffset, int bitCount)
                => _stringBlock.ReadUTF8(Read<int>(bitOffset, bitCount));
        }

        public sealed class InlinedStrings : ByteAlignedRecordReader
        {
            private readonly Encoding _encoding;

            public InlinedStrings(IBinaryStorageFile fileReader) : base()
            {
                _encoding = fileReader.Options.Encoding ?? Encoding.UTF8;
            }

            public override unsafe string ReadString(int bitOffset, int bitCount)
            {
                var startCursor = (byte*) Unsafe.AsPointer(ref _stagingBuffer[bitOffset / 8]);
                var stringLength = 0;
                while (startCursor[stringLength] != 0)
                    ++stringLength;

                return _encoding.GetString(startCursor, stringLength);
            }

            public override unsafe ReadOnlyMemory<byte> ReadUTF8(int bitOffset, int bitCount)
            {
                var startCursor = (byte*) Unsafe.AsPointer(ref _stagingBuffer[bitOffset / 8]);
                var stringLength = 0;
                while (startCursor[stringLength] != 0)
                    ++stringLength;

                return new ReadOnlyMemory<byte>(_stagingBuffer, bitOffset / 8, stringLength);
            }
        }
    }
}
