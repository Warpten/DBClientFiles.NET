using DBClientFiles.NET.Parsing.Versions;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers
{
    /// <summary>
    /// Abstract implementation of a block handler which is treating its binary data as a single structure.
    /// </summary>
    /// <typeparam name="TStructure">The structure read from the binary stream.</typeparam>
    internal abstract class StructuredBlockHandler<TStructure> : ISegmentHandler where TStructure : struct
    {
        public TStructure Structure { get; private set; } = default;

        public virtual unsafe void ReadSegment(IBinaryStorageFile reader, long startOffset, long length)
        {
            if (length == 0)
                return;

            Debug.Assert(length == Unsafe.SizeOf<TStructure>(), "Invalid structure size");
            Debug.Assert(reader.DataStream.Position == startOffset, "Out-of-place parsing!");

            var blockBytes = ArrayPool<byte>.Shared.Rent(Unsafe.SizeOf<TStructure>());

            // Size forced to ensure it's correct (Rent may return more)
            reader.DataStream.Read(blockBytes, 0, Unsafe.SizeOf<TStructure>());

            Structure = MemoryMarshal.Read<TStructure>(blockBytes);

            ArrayPool<byte>.Shared.Return(blockBytes);
        }

        public void WriteSegment<T, U>(T reader) where T : BinaryWriter, IWriter<U>
        {
            throw new NotImplementedException();
        }
    }
}
