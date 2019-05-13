using System;
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
        public Serializer() : base()
        {

        }

        protected override TypedSerializerGenerator<T, TypeDeserializer> Generator {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public override void Initialize(IBinaryStorageFile parser)
        {
            var recordBlock = parser.FindBlock(BlockIdentifier.FieldInfo)?.Handler as FieldInfoHandler<MemberMetadata>;
            
            base.Initialize(parser);
        }

        public int GetElementBitCount(MemberToken memberInfo)
        {
            throw new NotImplementedException();
        }

    }
}
