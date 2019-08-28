using DBClientFiles.NET.Parsing.Versions;
using System.IO;

namespace DBClientFiles.NET.Parsing.Shared.Segments
{
    internal interface ISegmentHandler
    {
        void ReadSegment(IBinaryStorageFile reader, long startOffset, long length);

        void WriteSegment<T, U>(T reader) where T : BinaryWriter, IWriter<U>;
    }
}
