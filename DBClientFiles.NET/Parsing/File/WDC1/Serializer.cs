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
        private TypedSerializerGenerator<T> Generator { get; set; }
        private TypeDeserializer TypeSerializer { get; set; }

        public Serializer() : base()
        {

        }

        public override void Initialize(IBinaryStorageFile storage)
        {            
            base.Initialize(storage);

            InfoBlock = storage.FindBlock(BlockIdentifier.FieldInfo)?.Handler as FieldInfoHandler<MemberMetadata>;

            var generator = new SerializerGenerator<T>(storage, InfoBlock);
            if (storage.Header.IndexTable.Exists)
                generator.IndexColumn = storage.Header.IndexColumn;

            Generator = generator;
        }

        public override T Deserialize(IRecordReader reader, IParser<T> parser)
        {
            if (TypeSerializer == null)
                TypeSerializer = Generator.GenerateDeserializer<TypeDeserializer>();

            Debug.Assert(TypeSerializer != null, "deserializer needed");

            TypeSerializer.Invoke(reader, out var instance);
            return instance;
        }
    }
}
