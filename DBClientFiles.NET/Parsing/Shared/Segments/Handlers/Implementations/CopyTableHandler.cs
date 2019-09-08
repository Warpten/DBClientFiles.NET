using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Utils.Extensions;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations
{
    internal sealed class CopyTableHandler : MultiDictionaryBlockHandler<int, int>
    {
        protected override int ReadKey(Stream dataStream) => dataStream.Read<int>();

        protected override int ReadValueElement(Stream dataStream) => dataStream.Read<int>();

        public override void WriteKey(BinaryWriter writer, int key)
        {
            throw new System.NotImplementedException();
        }

        public override void WriteValue(BinaryWriter writer, List<int> value)
        {
            throw new System.NotImplementedException();
        }
    }
}
