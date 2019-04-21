using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Reflection;
using System;

namespace DBClientFiles.NET.Parsing.File
{
    /// <summary>
    /// The basic interface representing a file.
    /// </summary>
    internal interface IBinaryStorageFile : IDisposable
    {
        TypeToken Type { get; }

        ref readonly StorageOptions Options { get; }

        Block FindBlock(BlockIdentifier identifier);

        IHeaderHandler Header { get; }
    }
}
