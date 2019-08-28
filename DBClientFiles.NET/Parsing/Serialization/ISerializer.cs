using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Versions;

namespace DBClientFiles.NET.Parsing.Serialization
{
    internal interface ISerializer<T>
    {
        /// <summary>
        /// Deserializes an instance of <see cref="{T}"/> from the provided stream.
        /// </summary>
        /// <param name="recordReader"></param>
        /// <param name="fileParser"></param>
        /// <returns></returns>
        T Deserialize(IRecordReader recordReader, IParser<T> fileParser);

        /// <summary>
        /// Given an instance of <see cref="{T}"/>, perform a deep copy operation and return a new object.
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        T Clone(in T origin);

        int GetRecordIndex(in T instance);
        void SetRecordIndex(out T instance, int index);

        void SetIndexColumn(int indexColumn);

        ref readonly StorageOptions Options { get; }

        void Initialize(IBinaryStorageFile storage);
    }
}
