using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DBClientFiles.NET.Parsing.Shared.Records
{
    /// <summary>
    /// An implementation of <see cref="ISequentialRecordReader"/>. The records
    /// within these files always have the same sizes. It is only able of performing aligned sequential reads.
    /// </summary>
    internal unsafe readonly struct AlignedSequentialRecordReader
    {
        public static class Methods
        {
            public static readonly MethodInfo Read = typeof(AlignedSequentialRecordReader).GetMethod("Read", new[] { typeof(Stream) });
            public static readonly MethodInfo ReadString = typeof(AlignedSequentialRecordReader).GetMethod("ReadString", new[] { typeof(Stream) });
        }

        private readonly StringBlockHandler _stringBlock;

        public AlignedSequentialRecordReader(StringBlockHandler stringBlock)
        {
            _stringBlock = stringBlock;
        }

        public void Dispose()
        {
        }

        public T Read<T>(Stream stream) where T : struct
        {
            var value = default(T);
            var valueSpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref value, 1));
            var readCount = stream.Read(valueSpan);
            if (readCount != Unsafe.SizeOf<T>())
                throw new InvalidOperationException($"Unable to read {typeof(T).Name} from file, got {readCount} bytes, needed {Unsafe.SizeOf<T>()}.");

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
