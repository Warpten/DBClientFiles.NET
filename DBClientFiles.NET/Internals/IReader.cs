using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using DBClientFiles.NET.Internals.Serializers;

namespace DBClientFiles.NET.Internals
{
    internal interface IReader<T> : IDisposable
        where T : class, new()
    {
        bool ReadHeader();
        void ReadSegments();
        IEnumerable<T> ReadRecords();
        
        StorageOptions Options { get; set; }
        CodeGenerator<T> Generator { get; }
    }
}
