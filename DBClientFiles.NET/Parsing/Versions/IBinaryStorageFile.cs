using DBClientFiles.NET.Parsing.Enumerators;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Shared.Headers;
using DBClientFiles.NET.Parsing.Shared.Segments;
using System;
using System.IO;

namespace DBClientFiles.NET.Parsing.Versions
{
    /// <summary>
    /// The basic interface representing a file.
    /// </summary>
    internal interface IBinaryStorageFile : IDisposable
    {
        TypeToken Type { get; }

        Stream DataStream { get; }

        /// <summary>
        /// The options to be used when processing the file.
        /// </summary>
        ref readonly StorageOptions Options { get; }

        /// <summary>
        /// Returns an instance of the provided segment handler type for the specified segment identifier, if one exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <returns></returns>
        T FindSegmentHandler<T>(SegmentIdentifier identifier) where T : ISegmentHandler;

        /// <summary>
        /// Returns a reference to a segment given a specific identifier.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        Segment FindSegment(SegmentIdentifier identifier);

        IHeaderAccessor Header { get; }
    }

    internal interface IBinaryStorageFile<T> : IBinaryStorageFile
    {
        IRecordEnumerator<T> GetEnumerator();
    }
}
