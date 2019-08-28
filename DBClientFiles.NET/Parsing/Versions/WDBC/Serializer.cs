using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Shared.Records;

namespace DBClientFiles.NET.Parsing.Versions.WDBC
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        private SerializerGenerator<T> Generator { get; set; }

        public override void Initialize(IBinaryStorageFile storage)
        {
            base.Initialize(storage);

            Generator = new SerializerGenerator<T>(storage.Type, storage.Options.TokenType);
        }

        public override T Deserialize(IRecordReader recordReader, IParser<T> fileParser)
        {
            Generator.Method.Invoke(recordReader, out var instance);
            return instance;
        }
    }
}
