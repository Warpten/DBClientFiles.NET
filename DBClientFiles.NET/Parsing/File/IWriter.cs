using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Parsing.File
{
    internal interface IWriter<T> : IDisposable
    {
        StorageOptions Options { get; }
        ISerializer<T> Serializer { get; }

        void Insert(T instance);
    }
}
