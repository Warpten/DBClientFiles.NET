using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.Serialization;
using System;

namespace DBClientFiles.NET.Parsing.File
{
    internal interface IWriter<T> : IBinaryStorageFile
    {
        ISerializer<T> Serializer { get; }
    }
}
