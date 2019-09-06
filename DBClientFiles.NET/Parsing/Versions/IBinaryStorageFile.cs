﻿using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using System;
using System.Collections.Generic;
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

        IHeaderHandler Header { get; }
    }

    internal interface IBinaryStorageFile<T> : IBinaryStorageFile, IEnumerable<T>
    {

    }
}
