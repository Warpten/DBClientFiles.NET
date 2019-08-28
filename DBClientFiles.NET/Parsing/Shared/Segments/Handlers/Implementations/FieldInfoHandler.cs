using DBClientFiles.NET.Parsing.Enums;
using DBClientFiles.NET.Parsing.Shared.Binding;
using System;
using System.IO;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations
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
                var previousIndex = _index - 1;
                T previousInstance = null;
                while ((previousInstance == null || previousInstance.CompressionData.Size == 0) && previousIndex >= 0)
                    previousInstance = this[previousIndex--];
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
