using System.Diagnostics;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Shared.Records;

namespace DBClientFiles.NET.Parsing.Versions.WDB5
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        private SerializerGenerator<T> Generator { get; set; }

        public override void Initialize(IBinaryStorageFile storage)
        {
            base.Initialize(storage);

            Generator = new SerializerGenerator<T>(storage);
        }

        public override T Deserialize(IRecordReader recordReader, IParser<T> fileParser)
        {
            Debug.Assert(recordReader is Parser<T>, "cross call not allowed for WDB5 serializer");

            Generator.Method.Invoke(recordReader, (Parser<T>) fileParser, out var instance);
            return instance;
        }

        
    }
}
