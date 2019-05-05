using System;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.Enums;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations
{
    internal class ExtendedFieldInfoHandler<T> : ListBlockHandler<T> where T : BaseMemberMetadata, new()
    {
        private int _index = 0;

        private IList<T> _fields;

        public ExtendedFieldInfoHandler(IList<T> preparedFields)
        {
            _fields = preparedFields;
        }

        protected override T ReadElement(BinaryReader reader)
        {
            var currentField = _fields[_index];
            ++_index;

            // Offset, in bits, of the field in the record. *Can* be zero for fields outside of the record (index table, relationship table)
            var fieldOffsetBits = reader.ReadUInt16();
            // Size, in bits, of the current member. For arrays, this is the entire size of the array, packed.
            var fieldSizeBits = reader.ReadUInt16();

            var additionalDataSize = reader.ReadInt32();

            currentField.CompressionData.Size = additionalDataSize; 

            // Retrieve the size from field info (byte-boundary) if it was defined
            // If *can* be zero for fields outside of the record (index table or relationship table)
            var fieldInfoSize = currentField.Size;

            currentField.Offset = fieldOffsetBits;

            currentField.Size = fieldSizeBits;

            currentField.CompressionData.Type = (MemberCompressionType)reader.ReadInt32();

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

                currentField.CompressionData.Offset = previousField != null
                    ? previousField.CompressionData.Offset + previousField.CompressionData.Size
                    : 0;
            }

            switch (currentField.CompressionData.Type)
            {
                case MemberCompressionType.SignedImmediate:
                case MemberCompressionType.Immediate:
                    {
                        _ = reader.ReadUInt32();
                        _ = reader.ReadUInt32();
                        var flags = reader.ReadUInt32();
                        if ((flags & 0x01) != 0 || currentField.CompressionData.Type == MemberCompressionType.SignedImmediate)
                            currentField.Properties |= MemberMetadataProperties.Signed;
                        break;
                    }
                case MemberCompressionType.CommonData:
                    {
                        currentField.RawDefaultValue = reader.ReadBytes(4);
                        _ = reader.ReadUInt32();
                        _ = reader.ReadUInt32();
                        break;
                    }
                case MemberCompressionType.BitpackedPalletArrayData:
                case MemberCompressionType.BitpackedPalletData:
                    {
                        _ = reader.ReadUInt32();
                        _ = reader.ReadUInt32();
                        var arrayCount = reader.ReadInt32();
                        if (currentField.CompressionData.Type == MemberCompressionType.BitpackedPalletArrayData)
                        {
                            currentField.Cardinality = arrayCount;
                            currentField.Size /= (uint) arrayCount;
                        }
                        break;
                    }
                default:
                    reader.BaseStream.Seek(3 * 4, SeekOrigin.Current);
                    break;
            }

            // fieldInfoSize == 0 means the field is outside the record (it's either a relationship column, or our index)
            // We don't bother forcing the calculation if arity was already specified in the field info.
            if (currentField.CompressionData.Type != MemberCompressionType.BitpackedPalletArrayData && fieldInfoSize != 0)
                currentField.Cardinality = (int) (currentField.Size / fieldInfoSize);

            return null;
        }

        protected override void WriteElement(BinaryWriter writer, in T element)
        {
            throw new NotImplementedException();
        }
    }
}
