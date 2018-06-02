using System;
using System.Diagnostics;
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

        private int _cardinality;
        public int Cardinality
        {
            get
            {
                if (CompressionOptions.CompressionType == MemberCompressionType.BitpackedPalletArrayData)
                    return CompressionOptions.Pallet.ArraySize;
                return _cardinality;
            }
            set => _cardinality = value;
        }

        public CompressionInfo CompressionOptions;

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct BitpackedInfo
        {
            [FieldOffset(0)] public int OffsetBits;
            [FieldOffset(4)] public int SizeBits;
            [FieldOffset(8)] public int Flags;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct CommonDataInfo
        {
            [FieldOffset(0)] public int DefaultValue;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct PalletInfo
        {
            [FieldOffset(0)] public int OffsetBits;
            [FieldOffset(4)] public int SizeBits;
            [FieldOffset(8)] public int ArraySize;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct CompressionInfo
        {
            [FieldOffset(0)] public int CompressedDataSize;
            [FieldOffset(4)] public MemberCompressionType CompressionType;
            [FieldOffset(8)] public BitpackedInfo Bitpacked;
            [FieldOffset(8)] public CommonDataInfo CommonData;
            [FieldOffset(8)] public PalletInfo Pallet;
        }

        public unsafe T GetDefaultValue<T>() where T : struct
        {
            Debug.Assert(CompressionOptions.CompressionType == MemberCompressionType.CommonData);

            Span<int> asInt = stackalloc int[1];
            asInt[0] = CompressionOptions.CommonData.DefaultValue;
            return MemoryMarshal.Cast<int, T>(asInt)[0];
        }
    }
}