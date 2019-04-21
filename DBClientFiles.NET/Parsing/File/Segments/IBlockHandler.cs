using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments
{
    internal interface IBlockHandler
    {
        BlockIdentifier Identifier { get; }

        void ReadBlock(BinaryReader reader, long startOffset, long length);

        void WriteBlock<T, U>(T reader) where T : BinaryWriter, IWriter<U>;
    }
}
