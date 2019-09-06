using DBClientFiles.NET.Parsing.Versions;
using System.IO;

namespace DBClientFiles.NET.Parsing.Shared.Headers
{
    internal interface IHeader
    {
        IBinaryStorageFile<T> MakeStorageFile<T>(in StorageOptions options, Stream dataStream);
    }
}
