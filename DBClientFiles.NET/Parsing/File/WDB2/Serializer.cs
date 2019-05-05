using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Serialization.Generators;

namespace DBClientFiles.NET.Parsing.File.WDB2
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

            // Reuse the WDBC generator because there are literally no changes
            Generator = new WDBC.SerializerGenerator<T, TypeDeserializer>(storage.Type, storage.Options.TokenType);
        }
    }
}
