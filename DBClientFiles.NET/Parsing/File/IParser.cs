using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Collections;
using System;
using System.Collections.Generic;
using DBClientFiles.NET.Internals.Binding;
using DBClientFiles.NET.Parsing.Binding;

namespace DBClientFiles.NET.Parsing.File
{
    internal interface IParser : IBinaryStorageFile
    {
        void Initialize();

        /// <summary>
        /// The total amount of records in the file (including copies)
        /// </summary>
        int Size { get; }

        BaseMemberMetadata GetFileMemberMetadata(int index);
    }

    internal interface IParser<T> : IParser
    {
        IEnumerable<T> Records { get; }
    }
}
