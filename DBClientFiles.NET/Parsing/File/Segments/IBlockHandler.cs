using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments
{
    internal interface IBlockHandler
    {
        BlockIdentifier Identifier { get; }

        void ReadBlock<T>(T reader, long startOffset, long length) where T : BinaryReader, IParser;

        void WriteBlock<T, U>(T reader) where T : BinaryWriter, IWriter<U>;
    }
}
