using System.Collections.Generic;

namespace DBClientFiles.NET.Parsing.File
{
    internal interface IParser<T> : IBinaryStorageFile, IEnumerable<T>
    {
        
    }
}
