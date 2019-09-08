using DBClientFiles.NET.Parsing.Enums;
using DBClientFiles.NET.Parsing.Shared.Binding;
using System;
using System.Diagnostics;
using System.IO;
using DBClientFiles.NET.Utils.Extensions;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations
{
    internal class FieldInfoHandler<T> : ListBlockHandler<T> where T : BaseMemberMetadata, new()
    {
        private int _index = 0;

        protected override T ReadElement(Stream dataStream)
        {
            var (collapsedBitCount, bytePosition) = dataStream.Read<(short, ushort)>();

            var instance = new T {
                Cardinality = 1
            };

            instance.CompressionData.Type = MemberCompressionType.Immediate;
            instance.Offset = bytePosition * 8;
            instance.Size = 32 - collapsedBitCount;

            if (_index > 0)
            {
                var previousIndex = _index - 1;
                T previousInstance = null;
                while ((previousInstance == null || previousInstance.Size == 0) && previousIndex >= 0)
                    previousInstance = this[previousIndex--];

                // Should never happen
                Debug.Assert(previousInstance != null);

                previousInstance.Cardinality = (int) ((instance.Offset - previousInstance.Offset) / previousInstance.Size);
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
