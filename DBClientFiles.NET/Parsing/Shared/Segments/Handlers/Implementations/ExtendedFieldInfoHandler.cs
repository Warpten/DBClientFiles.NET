using System;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Parsing.Enums;
using DBClientFiles.NET.Parsing.Shared.Binding;
using DBClientFiles.NET.Utils;
using DBClientFiles.NET.Utils.Extensions;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations
{
    internal class ExtendedFieldInfoHandler<T> : ListBlockHandler<T> where T : BaseMemberMetadata, new()
    {
        private int _index = 0;

        private readonly IList<T> _fields;

        public ExtendedFieldInfoHandler(IList<T> preparedFields)
        {
            _fields = preparedFields;
        }

        protected override T ReadElement(Stream dataStream)
        {
            var currentField = _fields[_index];
            ++_index;

            // Offset, in bits, of the field in the record. *Can* be zero for fields outside of the record (index table, relationship table)
            // Size, in bits, of the current member. For arrays, this is the entire size of the array, packed.
            var (fieldOffsetBits, fieldSizeBits, additionalDataSize, compressionType) = dataStream.Read<(ushort, ushort, int, MemberCompressionType)>();

            currentField.CompressionData.Type = compressionType;
            currentField.CompressionData.DataSize = additionalDataSize;

            // Retrieve the size from field info (byte-boundary) if it was defined
            // If *can* be zero for fields outside of the record (index table or relationship table)
            var fieldInfoSize = currentField.Size;
            currentField.Offset = fieldOffsetBits;
            currentField.Size = fieldSizeBits;


            if (_index > 1)
            {
                // Find the last field with the same compression group
                T previousField = null;
                for (var i = _index - 1; i >= 0; --i)
                {
                    if (_fields[i].CompressionData.Type.GetCompressionGroup() == currentField.CompressionData.Type.GetCompressionGroup())
                    {
                        previousField = _fields[i];
                        break;
                    }
                }

                currentField.CompressionData.DataOffset = previousField != null
                    ? previousField.CompressionData.DataOffset + previousField.CompressionData.DataSize
                    : 0;
            }

            switch (currentField.CompressionData.Type)
            {
                case MemberCompressionType.SignedImmediate:
                case MemberCompressionType.Immediate:
                {
                    var (_, _, flags) = dataStream.Read<(int, int, uint)>();
                      
                    if ((flags & 0x01) != 0 || currentField.CompressionData.Type == MemberCompressionType.SignedImmediate)
                        currentField.Properties |= MemberMetadataProperties.Signed;
                    break;
                }
                case MemberCompressionType.CommonData:
                {
                    var (defaultValue, _, _) = dataStream.Read<(Variant<int>, int, int)>();

                    currentField.DefaultValue = defaultValue;
                    break;
                }
                case MemberCompressionType.BitpackedPalletArrayData:
                case MemberCompressionType.BitpackedPalletData:
                {
                    var (_, _, arrayCount) = dataStream.Read<(int, int, int)>();
                    if (currentField.CompressionData.Type == MemberCompressionType.BitpackedPalletArrayData)
                    {
                        currentField.Cardinality = arrayCount;
                        currentField.Size /= arrayCount;
                    }
                    break;
                }
                default:
                    dataStream.Position += 3 * sizeof(int);
                    break;
            }

            // fieldInfoSize == 0 means the field is outside the record (it's either a relationship column, or our index)
            // We don't bother forcing the calculation if arity was already specified in the field info.
            if (currentField.CompressionData.Type != MemberCompressionType.BitpackedPalletArrayData && fieldInfoSize != 0)
                currentField.Cardinality = (int) (currentField.Size / fieldInfoSize);

            return currentField;
        }

        protected override void WriteElement(BinaryWriter writer, in T element)
        {
            throw new NotImplementedException();
        }
    }
}
