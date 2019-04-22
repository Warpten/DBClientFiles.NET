using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal abstract class StructuredBlockHandler<TStructure> : IBlockHandler where TStructure : struct
    {
        public TStructure Structure { get; private set; } = default;

        public virtual unsafe void ReadBlock(IBinaryStorageFile reader, long startOffset, long length)
        {
            if (length == 0)
                return;

            Debug.Assert(reader.BaseStream.Position == startOffset, "Out-of-place parsing!");

            Span<byte> blockBytes = stackalloc byte[Unsafe.SizeOf<TStructure>()];
            reader.BaseStream.Read(blockBytes);

            Structure = MemoryMarshal.Read<TStructure>(blockBytes);
        }

        public void WriteBlock<T, U>(T reader) where T : BinaryWriter, IWriter<U>
        {
            throw new NotImplementedException();
        }
    }
}
