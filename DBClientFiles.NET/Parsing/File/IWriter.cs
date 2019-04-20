using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.File
{
    internal interface IWriter<T> : IBinaryStorageFile
    {
        ISerializer<T> Serializer { get; }
    }
}
