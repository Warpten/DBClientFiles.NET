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
                Cardinality = 1
            };

            instance.CompressionData.Type = MemberCompressionType.Immediate;
            instance.CompressionData.Offset = bytePosition * 8;
            instance.CompressionData.Size = 32 - collapsedBitCount;

            if (_index > 0)
            {
                var previousInstance = this[_index - 1];
                previousInstance.Cardinality = (int) ((instance.CompressionData.Offset - previousInstance.CompressionData.Offset) / previousInstance.CompressionData.Size);
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
