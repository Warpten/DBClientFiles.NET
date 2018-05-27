using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;

namespace DBClientFiles.NET.Internals
{
    internal interface IReader<T>
    {
        bool ReadHeader();

        IEnumerable<T> ReadRecords();

        void ReadSegments();

        event Action<long, string> OnStringTableEntry;

        StorageOptions Options { get; set; }

        ExtendedMemberInfo[] Members { get; }
    }
}
