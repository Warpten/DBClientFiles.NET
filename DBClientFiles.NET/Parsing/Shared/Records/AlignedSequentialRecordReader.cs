using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DBClientFiles.NET.Parsing.Shared.Records
{
    /// <summary>
    /// An implementation of <see cref="ISequentialRecordReader"/>. The records
    /// within these files always have the same sizes. It is only able of performing aligned sequential reads.
    /// </summary>
    internal unsafe class AlignedSequentialRecordReader : ISequentialRecordReader
    {
        private readonly StringBlockHandler _stringBlock;

        public AlignedSequentialRecordReader(StringBlockHandler stringBlock)
        {
            _stringBlock = stringBlock;
        }

        public void Dispose()
        {
        }

        public T Read<T>(Stream stream) where T : unmanaged
        {
            var value = default(T);
            var valueSpan = new Span<T>(Unsafe.AsPointer(ref value), 1);
            var readCount = stream.Read(MemoryMarshal.AsBytes(valueSpan));
            if (readCount != sizeof(T))
                throw new InvalidOperationException($"Unable to read {typeof(T).Name} from file, got {readCount} bytes, needed {sizeof(T)}.");

            return value;
        }

        public string ReadString(Stream stream)
        {
            if (_stringBlock != null)
                return _stringBlock[Read<uint>(stream)];

            // This is going to be slow, but we hope it's not gonna be the hot path
            var sb = new StringBuilder(128);
            int @char;
            while ((@char = stream.ReadByte()) != '\0')
                sb.Append((char)@char);

            return sb.ToString();
        }
    }
}
