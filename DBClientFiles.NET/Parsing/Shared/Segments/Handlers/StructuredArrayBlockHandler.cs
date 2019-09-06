using DBClientFiles.NET.Parsing.Versions;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers
{
    internal abstract class StructuredArrayBlockHandler<TStructure> : ISegmentHandler where TStructure : struct
    {
        public abstract SegmentIdentifier Identifier { get; }

        public TStructure[] Structures { get; private set; } = null;

        public unsafe void ReadSegment(IBinaryStorageFile reader, long startOffset, long length)
        {
            if (length == 0)
                return;

            Debug.Assert(reader.DataStream.Position == startOffset, "Out-of-place parsing!");

            Structures = new TStructure[length / Unsafe.SizeOf<TStructure>()];

            var i = 0;
            var itemBytes = ArrayPool<byte>.Shared.Rent(Unsafe.SizeOf<TStructure>());
            while (reader.DataStream.Position < startOffset + length)
            {
                reader.DataStream.Read(itemBytes, 0, Unsafe.SizeOf<TStructure>());

                Structures[i] = MemoryMarshal.Read<TStructure>(itemBytes);
                ++i;
            }

            ArrayPool<byte>.Shared.Return(itemBytes);
        }

        public void WriteSegment<T, U>(T reader) where T : BinaryWriter, IWriter<U>
        {
            throw new NotImplementedException();
        }
    }
}
