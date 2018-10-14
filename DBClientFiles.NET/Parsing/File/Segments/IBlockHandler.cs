using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments
{
    internal interface IBlockHandler
    {
        BlockIdentifier Identifier { get; }

        void Parse<T, U>(T reader, long startOffset, long length) where T : BinaryReader, IReader<U>;
    }
}
