using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Collections;
using System;
using System.Collections.Generic;
using DBClientFiles.NET.Parsing.File.Segments;

namespace DBClientFiles.NET.Parsing.File
{
    internal interface IReader : IDisposable
    {
        IFileHeader Header { get; }
        StorageOptions Options { get; }

        U FindBlockHandler<U>(BlockIdentifier identifier) where U : IBlockHandler;
    }

    internal interface IReader<T> : IReader
    {
        IEnumerable<Proxy<T>> Records { get; }

        ISerializer<T> Serializer { get; }
    }
}
