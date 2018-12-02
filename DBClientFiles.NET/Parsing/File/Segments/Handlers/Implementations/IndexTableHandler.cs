using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations
{
    internal sealed class IndexTableHandler : ListBlockHandler<int>
    {
        public override BlockIdentifier Identifier { get; } = BlockIdentifier.IndexTable;

        protected override int ReadElement(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        protected override void WriteElement(BinaryWriter writer, in int element)
        {
            throw new System.NotImplementedException();
        }
    }

}
