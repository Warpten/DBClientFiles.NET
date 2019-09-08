using System;
using System.Collections.Generic;
using System.Text;

namespace DBClientFiles.NET.Collections.Generic
{
    public interface IRecordEnumerable<out T> : IEnumerable<T>, IDisposable
    {
        IRecordEnumerable<T> Skip(int skipCount);
        T ElementAt(int offset);
        T ElementAtOrDefault(int offset);
        T Last();
    }
}
