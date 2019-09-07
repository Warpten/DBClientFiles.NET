using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Shared.Records;
using System.IO;

namespace DBClientFiles.NET.Parsing.Versions.WDBC
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        private SerializerGenerator<T> Generator { get; set; }

        public Serializer(IBinaryStorageFile storage) : base(storage)
        {
            Generator = new SerializerGenerator<T>(storage.Type, storage.Options.TokenType);
        }

        public T Deserialize(Stream dataStream, in AlignedSequentialRecordReader recordReader)
        {
            Generator.Method.Invoke(dataStream, in recordReader, out var instance);
            return instance;
        }
    }
}
