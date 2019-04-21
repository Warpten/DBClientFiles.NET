using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments
{
    internal interface IBlockHandler
    {
        void ReadBlock(IBinaryStorageFile reader, long startOffset, long length);

        void WriteBlock<T, U>(T reader) where T : BinaryWriter, IWriter<U>;
    }
}
