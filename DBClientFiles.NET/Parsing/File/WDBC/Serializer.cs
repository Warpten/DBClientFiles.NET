using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Serialization.Generators;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        protected override TypedSerializerGenerator<T, TypeDeserializer> Generator { get; set; }

        public Serializer() : base()
        {
        }

        public override void Initialize(IBinaryStorageFile storage)
        {
            base.Initialize(storage);

            Generator = new SerializerGenerator<T, TypeDeserializer>(storage.Type, storage.Options.TokenType);
        }
    }
}
