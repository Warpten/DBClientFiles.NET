using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.File.Segments;
using System;

namespace DBClientFiles.NET.Parsing.File
{
    /// <summary>
    /// The basic interface representing a file.
    /// </summary>
    internal interface IBinaryStorageFile : IDisposable
    {
        IFileHeader Header { get; }

        StorageOptions Options { get; }

        U FindBlockHandler<U>(BlockIdentifier identifier) where U : IBlockHandler;
    }
}
