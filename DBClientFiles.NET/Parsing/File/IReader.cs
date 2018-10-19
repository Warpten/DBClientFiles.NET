using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Collections;
using System;
using System.Collections.Generic;
using DBClientFiles.NET.Parsing.File.Segments;

namespace DBClientFiles.NET.Parsing.File
{
    internal interface IReader<T> : IBinaryStorageFile
    {
        IEnumerable<Proxy<T>> Records { get; }

        ISerializer<T> Serializer { get; }
    }
}
