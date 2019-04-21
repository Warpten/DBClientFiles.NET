using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations
{
    internal sealed class IndexTableHandler : ListBlockHandler<int>
    {
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
