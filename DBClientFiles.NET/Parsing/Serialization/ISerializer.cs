using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.File;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.Reflection;

namespace DBClientFiles.NET.Parsing.Serialization
{
    internal interface ISerializer<T>
    {
        /// <summary>
        /// Deserializes an instance of <see cref="{T}"/> from the provided stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        T Deserialize(IRecordReader reader);

        /// <summary>
        /// Given an instance of <see cref="{T}"/>, perform a deep copy operation and return a new object.
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        T Clone(in T origin);

        int GetKey(in T instance);
        void SetKey(out T instance, int key);

        ref readonly StorageOptions Options { get; }

        void Initialize(IBinaryStorageFile storage);
    }
}
