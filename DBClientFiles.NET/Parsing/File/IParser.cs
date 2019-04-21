using System.Collections.Generic;
using DBClientFiles.NET.Parsing.Reflection;

namespace DBClientFiles.NET.Parsing.File
{
    internal interface IParser<T> : IBinaryStorageFile, IEnumerable<T>
    {
        
    }
}
