using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.Enums;
using System;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal class FieldInfoHandler<T> : ListBlockHandler<T> where T : BaseMemberMetadata, new()
    {
        private int _index = 0;

        protected override T ReadElement(BinaryReader reader)
        {
            var collapsedBitCount = reader.ReadInt16();
            var bytePosition = reader.ReadUInt16();

            var instance = new T {
                Size = (uint) (32 - collapsedBitCount),
                Offset = bytePosition * 8u,
                Cardinality = 1
            };

            instance.CompressionData.Type = MemberCompressionType.Immediate;

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
