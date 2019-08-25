using System;
using System.Collections.Generic;
using System.Diagnostics;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Serialization.Generators;

namespace DBClientFiles.NET.Parsing.File.WDB5
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        private delegate void TypeDeserializer(IRecordReader recordReader, IParser<T> fileParser, out T instance);

        private TypedSerializerGenerator<T> Generator { get; set; }
        private TypeDeserializer TypeSerializer { get; set; }

        public Serializer() : base()
        {

        }

        public override void Initialize(IBinaryStorageFile storage)
        {
            base.Initialize(storage);

            var fieldMembers = storage.FindBlock(BlockIdentifier.FieldInfo)?.Handler as FieldInfoHandler<MemberMetadata>;
            var generator = new SerializerGenerator<T>(storage.Type, storage.Options.TokenType, fieldMembers);

            if (storage.Header.IndexTable.Exists)
                generator.SetIndexColumn(storage.Header.IndexColumn);

            Generator = generator;
        }

        public override T Deserialize(IRecordReader reader, IParser<T> parser)
        {
            if (TypeSerializer == null)
                TypeSerializer = Generator.GenerateDeserializer<TypeDeserializer>();

            Debug.Assert(TypeSerializer != null, "deserializer needed");

            TypeSerializer.Invoke(reader, parser, out var instance);
            return instance;
        }

        
    }
}
