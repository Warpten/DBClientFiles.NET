using System.Collections.Generic;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    internal interface IRecordEnumerator<out T> : IEnumerator<T>
    {
        void Skip(int skipCount);

        T ElementAt(int index);

        T ElementAtOrDefault(int index);

        T Last();
    }
}