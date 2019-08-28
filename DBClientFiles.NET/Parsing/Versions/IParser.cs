using System.Collections.Generic;

namespace DBClientFiles.NET.Parsing.Versions
{
    internal interface IParser<T> : IBinaryStorageFile, IEnumerable<T>
    {
        
    }
}
