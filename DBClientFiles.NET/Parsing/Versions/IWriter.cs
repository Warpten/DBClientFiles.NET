using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.Versions
{
    internal interface IWriter<T> : IBinaryStorageFile
    {
        ISerializer<T> Serializer { get; }
    }
}
