using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal abstract class StructuredArrayBlockHandler<TStructure> : IBlockHandler where TStructure : struct
    {
        public abstract BlockIdentifier Identifier { get; }

        public TStructure[] Structures { get; private set; } = null;

        public unsafe void ReadBlock(BinaryReader reader, long startOffset, long length)
        {
            if (length == 0)
                return;

            Debug.Assert(reader.BaseStream.Position == startOffset, "Out-of-place parsing!");

            Structures = new TStructure[length / Unsafe.SizeOf<TStructure>()];

            var i = 0;
            while (reader.BaseStream.Position < startOffset + length)
            {
                Span<byte> itemBytes = stackalloc byte[Unsafe.SizeOf<TStructure>()];
                reader.Read(itemBytes);

                Structures[i] = MemoryMarshal.Read<TStructure>(itemBytes);
                ++i;
            }
        }

        public void WriteBlock<T, U>(T reader) where T : BinaryWriter, IWriter<U>
        {
            throw new NotImplementedException();
        }
    }
}
