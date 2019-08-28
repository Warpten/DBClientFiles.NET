using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations
{
    internal sealed class CopyTableHandler : MultiDictionaryBlockHandler<int, int>
    {
        public override int ReadKey(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        public override int ReadValueElement(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

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
