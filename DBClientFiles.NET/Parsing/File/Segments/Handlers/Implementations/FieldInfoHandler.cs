using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.Enums;
using System;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal class FieldInfoHandler<T> : ListBlockHandler<T> where T : BaseMemberMetadata, new()
    {
        public override BlockIdentifier Identifier { get; } = BlockIdentifier.FieldInfo;

        private int _index = 0;

        protected override T ReadElement(BinaryReader reader)
        {
            var instance = new T {
                CompressionType = MemberCompressionType.None,
                CompressionIndex = (uint) _index,
                Size = (uint) (32 - reader.ReadInt16()),
                Offset = reader.ReadUInt32()
            };

            if (_index > 0)
            {
                var previousInstance = this[_index - 1];
                previousInstance.Cardinality = (int) ((instance.Offset - previousInstance.Offset) / previousInstance.Cardinality);
            }

            ++_index;

            return instance;
        }

        protected override void WriteElement(BinaryWriter writer, in T element)
        {
            throw new NotImplementedException();
        }
    }
}
