using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using System;
using System.IO;

namespace DBClientFiles.NET.Parsing.Versions
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

        Segment FindSegment(SegmentIdentifier identifier);

        IHeaderHandler Header { get; }

        Stream BaseStream { get; }
    }
}
