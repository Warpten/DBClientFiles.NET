using System.Diagnostics;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Shared.Records;

namespace DBClientFiles.NET.Parsing.Versions.WDB5
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        private SerializerGenerator<T> Generator { get; set; }

        public Serializer(IBinaryStorageFile storage) : base(storage)
        {
            Generator = new SerializerGenerator<T>(storage);
        }

        public T Deserialize(IRecordReader recordReader, IBinaryStorageFile<T> fileParser)
        {
            Debug.Assert(fileParser is StorageFile<T>, "cross call not allowed for WDB5 serializer");

            Generator.Method.Invoke(recordReader, out var instance);
            return instance;
        }
    }
}
