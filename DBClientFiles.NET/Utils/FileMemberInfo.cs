using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using DBClientFiles.NET.Internals;

namespace DBClientFiles.NET.Utils
{
    /// <summary>
    /// This class representes an element described in a given DBC|DB2 file.
    /// </summary>
    internal class FileMemberInfo
    {
        /// <summary>
        /// Size, in bytes, of this field.
        /// </summary>
        public int ByteSize { get; set; }

        /// <summary>
        /// Size, in bits, of this field.
        /// </summary>
        public int BitSize  { get; set; }

        /// <summary>
        /// Offset, in bits, of this field in the record.
        /// </summary>
        public int Offset   { get; set; }
        
        /// <summary>
        /// Index of this field in the file metainfo.
        /// </summary>
        public int Index    { get; set; }

        /// <summary>
        /// This is the index of the field in file metainfo, based on <see cref="CompressionType"/>.
        /// </summary>
        public int CategoryIndex { get; set; }

        /// <summary>
        /// Cardinality of the field, defined by file metadata. This is guessed for old formats.
        /// </summary>
        public int Cardinality { get; set; } = 1;

        public int CompressedDataSize { get; set; }
        public MemberCompressionType CompressionType { get; set; }

        /// <summary>
        /// If <see cref="CompressionType"/> is <see cref="MemberCompressionType.CommonData"/>, this is set. null otherwise.
        /// </summary>
        public byte[] DefaultValue { get; set; }
        public bool? IsSigned { get; set; }

        public T GetDefaultValue<T>() where T : struct
        {
            Debug.Assert(CompressionType == MemberCompressionType.CommonData);

            var asSpan = DefaultValue.AsSpan();
            return MemoryMarshal.Read<T>(asSpan);
        }

        public void Initialize(BinaryReader reader)
        {
            ByteSize = 4 - reader.ReadInt16() / 8;
            Offset = reader.ReadInt16() * 8;
        }

        public void ReadExtra(BinaryReader reader)
        {
            Offset = reader.ReadInt16();
            BitSize = reader.ReadInt16();

            CompressedDataSize = reader.ReadInt32();
            CompressionType = (MemberCompressionType)reader.ReadInt32();

            switch (CompressionType)
            {
                case MemberCompressionType.Immediate:
                    reader.BaseStream.Seek(4 + 4, SeekOrigin.Current);
                    IsSigned = (reader.ReadInt32() & 0x01) != 0;
                    break;
                case MemberCompressionType.SignedImmediate:
                    IsSigned = true;
                    goto case MemberCompressionType.None;
                case MemberCompressionType.CommonData:
                    DefaultValue = reader.ReadBytes(4);
                    reader.BaseStream.Seek(4 + 4, SeekOrigin.Current);
                    break;
                case MemberCompressionType.BitpackedPalletArrayData:
                    reader.BaseStream.Seek(4 + 4, SeekOrigin.Current);
                    Cardinality = reader.ReadInt32();
                    break;
                case MemberCompressionType.BitpackedPalletData:
                case MemberCompressionType.None:
                    reader.BaseStream.Seek(4 + 4 + 4, SeekOrigin.Current);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (ByteSize != 0 && CompressionType != MemberCompressionType.BitpackedPalletArrayData)
                Cardinality = BitSize / (8 * ByteSize);
        }
    }
}