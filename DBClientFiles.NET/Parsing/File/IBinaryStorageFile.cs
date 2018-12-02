using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.Reflection;
using System;

namespace DBClientFiles.NET.Parsing.File
{
    /// <summary>
    /// The basic interface representing a file.
    /// </summary>
    internal interface IBinaryStorageFile : IDisposable
    {
        TypeInfo Type { get; }

        ref readonly IFileHeader Header { get; }

        ref readonly StorageOptions Options { get; }

        U FindBlockHandler<U>(BlockIdentifier identifier) where U : IBlockHandler;
    }
}
