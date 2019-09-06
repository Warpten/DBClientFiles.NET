using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions.WDC1.Binding;

namespace DBClientFiles.NET.Parsing.Versions.WDC1
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        private delegate void TypeDeserializer(IRecordReader recordReader, out T instance);

        private FieldInfoHandler<MemberMetadata> InfoBlock { get; set; }

        private SerializerGenerator<T> Generator { get; set; }

        public Serializer(IBinaryStorageFile storage) : base(storage)
        {
            InfoBlock = storage.FindSegment(SegmentIdentifier.FieldInfo)?.Handler as FieldInfoHandler<MemberMetadata>;

            Generator = new SerializerGenerator<T>(storage, InfoBlock);
        }

        public T Deserialize(IRecordReader recordReader)
        {
            Generator.Method.Invoke(recordReader, out var instance);
            return instance;
        }
    }
}
