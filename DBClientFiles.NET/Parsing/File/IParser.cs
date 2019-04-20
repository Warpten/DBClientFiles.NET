using System.Collections.Generic;

namespace DBClientFiles.NET.Parsing.File
{
    internal interface IParser : IBinaryStorageFile
    {
        /// <summary>
        /// The total amount of records in the file.
        /// </summary>
        int RecordCount { get; }
    }

    internal interface IParser<T> : IParser, IEnumerable<T>
    {
    }
}
