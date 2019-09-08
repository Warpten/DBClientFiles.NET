using System.IO;
using DBClientFiles.NET.Utils.Extensions;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations
{
    internal sealed class IndexTableHandler : ListBlockHandler<int>
    {
        protected override int ReadElement(Stream dataStream) => dataStream.Read<int>();

        protected override void WriteElement(BinaryWriter writer, in int element)
        {
            throw new System.NotImplementedException();
        }
    }

}
