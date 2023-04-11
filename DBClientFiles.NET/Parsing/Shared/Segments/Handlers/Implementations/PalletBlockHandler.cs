using DBClientFiles.NET.Parsing.Versions;
using DBClientFiles.NET.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations
{
    internal class PalletBlockHandler : ISegmentHandler
    {
        private IMemoryOwner<byte> _rawValue;

        public T Read<T>(int byteOffset) where T : struct
            => MemoryMarshal.Read<T>(_rawValue.Memory.Span.Slice(byteOffset));

        public T[] ReadArray<T>(int byteOffset, int arraySize) where T : struct
            => MemoryMarshal.Cast<byte, T>(_rawValue.Memory.Span[byteOffset..])[..arraySize].ToArray();

        #region ISegmentHandler
        public void ReadSegment(IBinaryStorageFile reader, long startOffset, long length)
        {
            reader.DataStream.Position = startOffset;

            _rawValue = MemoryPool<byte>.Shared.Rent((int) length);
            reader.DataStream.Read(_rawValue.Memory.Span);
        }

        public void WriteSegment<T, U>(T reader) where T : BinaryWriter, IWriter<U>
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
