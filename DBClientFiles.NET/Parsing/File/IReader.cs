using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Collections;
using System;
using System.Collections.Generic;
using DBClientFiles.NET.Parsing.File.Segments;

namespace DBClientFiles.NET.Parsing.File
{
    internal interface IReader<T> : IBinaryStorageFile
    {
        void Initialize();

        /// <summary>
        /// The total amount of records in the file (including copies)
        /// </summary>
        int Size { get; }
        IEnumerable<T> Records { get; }
        ISerializer<T> Serializer { get; }
    }
}
