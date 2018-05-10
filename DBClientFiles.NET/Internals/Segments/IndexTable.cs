using DBClientFiles.NET.Internals.Versions;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;
using System.Collections.Generic;
using System.IO;
using BinaryReader = DBClientFiles.NET.IO.BinaryReader;

namespace DBClientFiles.NET.Internals.Segments
{
    internal class IndexTable : Segment
    {
        private uint[] Index { get; set; }

        public override void Read(BaseReader reader)
        {
            Index = new uint[Length / 4];

            if (Length == 0)
                return;

            reader.BaseStream.Position = StartOffset;
            for (var i = 0; i < Index.Length; ++i)
                Index[i] = reader.ReadUInt32();
        }

        public override void Dispose()
        {
            base.Dispose();

            Index = null;
        }

        public uint this[int index] => Index[index];
    }
}
