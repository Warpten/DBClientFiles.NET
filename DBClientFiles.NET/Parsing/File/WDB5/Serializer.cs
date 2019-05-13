using System;
using System.Collections.Generic;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Serialization.Generators;

namespace DBClientFiles.NET.Parsing.File.WDB5
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        private IList<MemberMetadata> _fieldMembers;

        public Serializer() : base()
        {

        }

        protected override TypedSerializerGenerator<T, TypeDeserializer> Generator { get; set; }

        public override void Initialize(IBinaryStorageFile storage)
        {
            var fieldMembers = storage.FindBlock(BlockIdentifier.FieldInfo)?.Handler as FieldInfoHandler<MemberMetadata>;

            Generator = new SerializerGenerator<T, TypeDeserializer>(storage.Type, storage.Options.TokenType, fieldMembers);
            
            base.Initialize(storage);
        }

        public int GetElementBitCount(MemberToken memberInfo)
        {
            throw new NotImplementedException();
        }

    }
}
