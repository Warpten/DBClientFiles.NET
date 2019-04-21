using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Reflection;
using System;
using System.IO;

namespace DBClientFiles.NET.Parsing.File
{
    /// <summary>
    /// The basic interface representing a file.
    /// </summary>
    internal interface IBinaryStorageFile : IDisposable
    {
        /// <summary>
        /// The total amount of records in the file.
        /// </summary>
        int RecordCount { get; }

        TypeToken Type { get; }

        ref readonly StorageOptions Options { get; }

        Block FindBlock(BlockIdentifier identifier);

        IHeaderHandler Header { get; }

        Stream BaseStream { get; }
    }
}
