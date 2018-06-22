using DBClientFiles.NET.Internals.Versions;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments;

namespace DBClientFiles.NET.Internals.Generators
{
    internal interface IGenerator<TKey, T> : IGenerator<T>
    {
        TKey ExtractKey(T instance);

        void InsertKey(T instance, TKey keyValue);

        T Deserialize(TKey forcedKeyValue, RecordReader recordReader);
        void Deserialize(T instance, TKey forcedKeyValue, RecordReader recordReader);
    }

    internal interface IGenerator<T>
    {
        T Deserialize(RecordReader recordReader);
        void Deserialize(T instance, RecordReader recordReader);

        T Clone(T origin);

        FileReader Reader { get; }
        IFileHeader Header { get; }
        StorageOptions Options { get; }
    }
}
