using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Utils.Extensions;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace DBClientFiles.NET.Parsing.Shared.Records
{
    /// <summary>
    /// A stream reader able to only read non-packed sequential values.
    /// </summary>
    internal readonly struct AlignedSequentialRecordReader
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(Stream stream) where T : struct
        {
            return stream.Read<T>();
        }

        public string ReadString(Stream stream)
        {
            if (_stringBlock != null)
                return _stringBlock[stream.Read<uint>()];

            // This is going to be slow, but we hope it's not gonna be the hot path
            var sb = new StringBuilder(128);
            int @char;
            while ((@char = stream.ReadByte()) != '\0')
                sb.Append((char)@char);

            return sb.ToString();
        }
    }
}
