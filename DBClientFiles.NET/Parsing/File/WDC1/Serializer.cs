using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Serialization.Generators;

namespace DBClientFiles.NET.Parsing.File.WDC1
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        private TypeMapper _mapper;

        public Serializer() : base()
        {

        }

        protected override TypedSerializerGenerator<T, TypeDeserializer> Generator {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public override void Initialize(IBinaryStorageFile parser)
        {
            _mapper = new TypeMapper(parser.Type);
            var infoBlock = parser.FindBlock(BlockIdentifier.FieldInfo)?.Handler as FieldInfoHandler<MemberMetadata>;

            // _mapper.Resolve(parser.Options.MemberType.ToTypeToken(), infoBlock);

            base.Initialize(parser);
        }
    }
}
