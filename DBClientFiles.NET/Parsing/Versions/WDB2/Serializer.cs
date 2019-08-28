using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Shared.Records;

namespace DBClientFiles.NET.Parsing.Versions.WDB2
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {

        private WDBC.SerializerGenerator<T> Generator { get; set; }

        public Serializer() : base()
        {

        }

        public override void Initialize(IBinaryStorageFile storage)
        {
            base.Initialize(storage);

            // Reuse the WDBC generator because there are literally no changes
            Generator = new WDBC.SerializerGenerator<T>(storage.Type, storage.Options.TokenType);
        }

        public override T Deserialize(IRecordReader recordReader, IParser<T> fileParser)
        {
            Generator.Method.Invoke(recordReader, out var instance);
            return instance;
        }
    }
}
