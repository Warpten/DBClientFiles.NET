using System.Diagnostics;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.File.WDB5
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        private SerializerGenerator<T> Generator { get; set; }

        public override void Initialize(IBinaryStorageFile storage)
        {
            base.Initialize(storage);

            Generator = new SerializerGenerator<T>(storage);
        }

        public override T Deserialize(IRecordReader reader, IParser<T> parser)
        {
            Debug.Assert(reader is Parser<T>, "cross call not allowed for WDB5 serializer");

            Generator.Method.Invoke(reader, (Parser<T>) parser, out var instance);
            return instance;
        }

        
    }
}
