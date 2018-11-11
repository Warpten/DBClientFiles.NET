using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.File;
using DBClientFiles.NET.Parsing.File.Records;

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
        /// Deserializes an instance of <see cref="{T}"/> from the provided stream.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="reader"></param>
        // void Deserialize(T instance, RecordReader reader);

        /// <summary>
        /// Given an instance of <see cref="{T}"/>, perform a deep copy operation and return a new object.
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        T Clone(T origin);

        StorageOptions Options { get; }
    }

    internal interface ISerializer<TKey, TValue> : ISerializer<TValue>
    {
        TKey GetKey(TValue instance);

        void SetKey(TValue instance, TKey key);
    }
}
