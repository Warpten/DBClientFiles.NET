using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Serialization.Generators;
using System.Diagnostics;

namespace DBClientFiles.NET.Parsing.File.WDC1
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        private delegate void TypeDeserializer(IRecordReader recordReader, out T instance);

        private FieldInfoHandler<MemberMetadata> InfoBlock { get; set; }

        private SerializerGenerator<T> Generator { get; set; }

        public Serializer() : base()
        {

        }

        public override void Initialize(IBinaryStorageFile storage)
        {            
            base.Initialize(storage);

            InfoBlock = storage.FindBlock(BlockIdentifier.FieldInfo)?.Handler as FieldInfoHandler<MemberMetadata>;

            Generator = new SerializerGenerator<T>(storage, InfoBlock);
        }

        public override T Deserialize(IRecordReader reader, IParser<T> parser)
        {
            Generator.Method.Invoke(reader, out var instance);
            return instance;
        }
    }
}
