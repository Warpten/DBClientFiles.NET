using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        private SerializerGenerator<T> Generator { get; set; }

        public override void Initialize(IBinaryStorageFile storage)
        {
            base.Initialize(storage);

            Generator = new SerializerGenerator<T>(storage.Type, storage.Options.TokenType);
        }

        public override T Deserialize(IRecordReader reader, IParser<T> parser)
        {
            Generator.Method.Invoke(reader, out var instance);
            return instance;
        }
    }
}
