using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DBClientFiles.NET.IO
{
    /// <summary>
    /// Metainformation for <see cref="RecordReader"/>.
    /// </summary>
    public static class _RecordReader
    {
        private static Type[] arrayArgs { get; } = new[] { typeof(int) };
        private static Type[] arrayPackedArgs { get; } = new[] { typeof(int), typeof(int), typeof(int) };
        private static Type[] packedArgs { get; } = new[] { typeof(int), typeof(int) };

        public static MethodInfo ReadPackedUInt64  { get; } = typeof(RecordReader).GetMethod("ReadUInt64", packedArgs);
        public static MethodInfo ReadPackedUInt32  { get; } = typeof(RecordReader).GetMethod("ReadUInt32", packedArgs);
        public static MethodInfo ReadPackedUInt16  { get; } = typeof(RecordReader).GetMethod("ReadUInt16", packedArgs);
        public static MethodInfo ReadPackedSByte   { get; } = typeof(RecordReader).GetMethod("ReadSByte",  packedArgs);

        public static MethodInfo ReadPackedInt64   { get; } = typeof(RecordReader).GetMethod("ReadInt64", packedArgs);
        public static MethodInfo ReadPackedInt32   { get; } = typeof(RecordReader).GetMethod("ReadInt32", packedArgs);
        public static MethodInfo ReadPackedInt16   { get; } = typeof(RecordReader).GetMethod("ReadInt16", packedArgs);
        public static MethodInfo ReadPackedByte    { get; } = typeof(RecordReader).GetMethod("ReadByte",  packedArgs);

        public static MethodInfo ReadPackedString  { get; } = typeof(RecordReader).GetMethod("ReadString", packedArgs);
        public static MethodInfo ReadPackedStrings { get; } = typeof(RecordReader).GetMethod("ReadStrings", arrayPackedArgs);
        public static MethodInfo ReadPackedArray   { get; } = typeof(RecordReader).GetMethod("ReadArray", arrayPackedArgs);

        public static MethodInfo ReadUInt64        { get; } = typeof(RecordReader).GetMethod("ReadUInt64", Type.EmptyTypes);
        public static MethodInfo ReadUInt32        { get; } = typeof(RecordReader).GetMethod("ReadUInt32", Type.EmptyTypes);
        public static MethodInfo ReadUInt16        { get; } = typeof(RecordReader).GetMethod("ReadUInt16", Type.EmptyTypes);
        public static MethodInfo ReadSByte         { get; } = typeof(RecordReader).GetMethod("ReadSByte", Type.EmptyTypes);

        public static MethodInfo ReadInt64         { get; } = typeof(RecordReader).GetMethod("ReadInt64", Type.EmptyTypes);
        public static MethodInfo ReadInt32         { get; } = typeof(RecordReader).GetMethod("ReadInt32", Type.EmptyTypes);
        public static MethodInfo ReadInt16         { get; } = typeof(RecordReader).GetMethod("ReadInt16", Type.EmptyTypes);
        public static MethodInfo ReadByte          { get; } = typeof(RecordReader).GetMethod("ReadByte", Type.EmptyTypes);

        public static MethodInfo ReadSingle        { get; } = typeof(RecordReader).GetMethod("ReadSingle", Type.EmptyTypes);
        public static MethodInfo ReadString        { get; } = typeof(RecordReader).GetMethod("ReadString", Type.EmptyTypes);
        public static MethodInfo ReadStrings       { get; } = typeof(RecordReader).GetMethod("ReadStrings", arrayArgs);
        public static MethodInfo ReadArray         { get; } = typeof(RecordReader).GetMethod("ReadArray", arrayArgs);

        public static Dictionary<TypeCode, MethodInfo> PackedReaders { get; } = new Dictionary<TypeCode, MethodInfo>()
        {
            { TypeCode.UInt64, ReadPackedUInt64 },
            { TypeCode.UInt32, ReadPackedUInt32 },
            { TypeCode.UInt16, ReadPackedUInt16 },
            { TypeCode.SByte, ReadPackedSByte },

            { TypeCode.Int64, ReadPackedInt64 },
            { TypeCode.Int32, ReadPackedInt32 },
            { TypeCode.Int16, ReadPackedInt16 },
            { TypeCode.Byte, ReadPackedByte },

            { TypeCode.String, ReadPackedString },
        };

        public static Dictionary<TypeCode, MethodInfo> Readers { get; } = new Dictionary<TypeCode, MethodInfo>()
        {
            { TypeCode.UInt64, ReadUInt64 },
            { TypeCode.UInt32, ReadUInt32 },
            { TypeCode.UInt16, ReadUInt16 },
            { TypeCode.SByte, ReadSByte },

            { TypeCode.Int64, ReadInt64 },
            { TypeCode.Int32, ReadInt32 },
            { TypeCode.Int16, ReadInt16 },
            { TypeCode.Byte, ReadByte },

            { TypeCode.String, ReadString },
            { TypeCode.Single, ReadSingle },
        };
    }

    /// <summary>
    /// This class acts as a thing wrapper around the record data for a row. It can read either packed or unpacked elements.
    /// </summary>
    internal unsafe class RecordReader : IDisposable
    {
        private byte[] _recordData;

        protected int _byteCursor = 0;

        public long ReadInt64() => Read<long>(_byteCursor, true);
        public int ReadInt32() => Read<int>(_byteCursor, true);
        public short ReadInt16() => Read<short>(_byteCursor, true);
        public byte ReadByte() => Read<byte>(_byteCursor, true);
        
        public ulong ReadUInt64() => Read<ulong>(_byteCursor, true);
        public uint ReadUInt32() => Read<uint>(_byteCursor, true);
        public ushort ReadUInt16() => Read<ushort>(_byteCursor, true);
        public sbyte ReadSByte() => Read<sbyte>(_byteCursor, true);

        public float ReadSingle() => Read<float>(_byteCursor, true);

        protected FileReader _fileReader;
        protected bool _usesStringTable;

        public int StartOffset { get; }

        public RecordReader(FileReader fileReader, bool usesStringTable, int recordSize)
        {
            StartOffset = (int)fileReader.BaseStream.Position;

            _usesStringTable = usesStringTable;
            _fileReader = fileReader;
            using (var reader = new BinaryReader(fileReader.BaseStream, Encoding.UTF8, true))
                _recordData = reader.ReadBytes(recordSize);
        }

        public long ReadInt64(int bitOffset, int bitCount)
        {
            _byteCursor = bitOffset + bitCount;

            var longValue = Read<long>(bitOffset) >> (bitOffset & 7);
            if (bitCount != 64)
                longValue &= (1L << bitCount) - 1;

            return longValue;
        }

        public ulong ReadUInt64(int bitOffset, int bitCount)
        {
            _byteCursor = bitOffset + bitCount;

            var longValue = Read<ulong>(bitOffset) >> (bitOffset & 7);
            if (bitCount != 64)
                longValue &= (1uL << bitCount) - 1;

            return longValue;
        }

        public int ReadInt32(int bitOffset, int bitCount)
        {
            _byteCursor = bitOffset + bitCount;

            var intValue = Read<int>(bitOffset) >> (bitOffset & 7);
            if (bitCount != 32)
                intValue &= (1 << bitCount) - 1;

            return intValue;
        }

        public uint ReadUInt32(int bitOffset, int bitCount)
        {
            _byteCursor = bitOffset + bitCount;

            var intValue = Read<uint>(bitOffset) >> (bitOffset & 7);
            if (bitCount != 32)
                intValue &= (1u << bitCount) - 1;

            return intValue;
        }

        public short ReadInt16(int bitOffset, int bitCount)
        {
            _byteCursor = bitOffset + bitCount;

            var shortValue = Read<short>(bitOffset) >> (bitOffset & 7);
            if (bitCount != 16)
                shortValue &= (1 << bitCount) - 1;

            return (short)shortValue;
        }

        public ushort ReadUInt16(int bitOffset, int bitCount)
        {
            _byteCursor = bitOffset + bitCount;

            var shortValue = Read<short>(bitOffset) >> (bitOffset & 7);
            if (bitCount != 16)
                shortValue &= (1 << bitCount) - 1;

            return (ushort)shortValue;
        }

        public byte ReadByte(int bitOffset, int bitCount)
        {
            _byteCursor = bitOffset + bitCount;

            var byteValue = Read<byte>(bitOffset) >> (bitOffset & 7);
            if (bitCount != 8)
                byteValue &= (1 << bitCount) - 1;

            return (byte)byteValue;
        }

        public sbyte ReadSByte(int bitOffset, int bitCount)
        {
            _byteCursor = bitOffset + bitCount;

            var byteValue = Read<sbyte>(bitOffset) >> (bitOffset & 7);
            if (bitCount != 8)
                byteValue &= (1 << bitCount) - 1;

            return (sbyte)byteValue;
        }

        public float ReadSingle(int bitOffset)
        {
            _byteCursor += 32;
            return Read<float>(bitOffset);
        }

        /// <summary>
        /// Returns an instance of the unmanaged type <see cref="{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bitOffset">The absolute offset (in bits) at which the type to read is. If set to zero, <see cref="RecordReader"/> assumes to be sequentially reading from the previous call.</param>
        /// <returns></returns>
        /// <remarks>
        /// While this may look fine, it will return a value that will be unaccurate unless properly shifted to the right by <code><paramref name="bitOffset"/> & 7</code>, as this cannot be typically done by this method.
        /// </remarks>
        private T Read<T>(int bitOffset, bool advanceCursor = false) where T : struct
        {
            T v;
            fixed (byte* dataBlock = _recordData)
                v = FastStructure<T>.PtrToStructure(new IntPtr(dataBlock + bitOffset / 8));

            if (advanceCursor)
                _byteCursor += SizeCache<T>.Size * 8;

            return v;
        }

        /// <summary>
        /// Reads a string from the record.
        /// </summary>
        /// <returns></returns>
        public virtual string ReadString()
        {
            if (_usesStringTable)
                return _fileReader.FindStringByOffset(ReadInt32());

            return _fileReader.ReadString();
        }

        /// <summary>
        /// Reads a string from the record, given the provided bit offset and length.
        /// </summary>
        /// <exception cref="InvalidOperationException">This exception is thrown when the caller tries to read a packed string offset in a file which does not have a string table.</exception>
        /// <param name="bitOffset">The absolute offset in the structure, in bits, at which the string is located.</param>
        /// <param name="bitCount">The amount of bits in which the offset to the string is contained.</param>
        /// <returns></returns>
        public virtual string ReadString(int bitOffset, int bitCount)
        {
            if (_usesStringTable)
                return _fileReader.FindStringByOffset(ReadInt32(bitOffset, bitCount));

            if ((bitOffset & 7) == 0)
                return _fileReader.ReadString();

            throw new InvalidOperationException("Packed strings must be in the string block!");
        }
        
        public long ReadBits(int bitOffset, int bitCount)
        {
            var byteOffset = bitOffset / 8;
            var byteCount = (bitCount + (bitOffset & 7) + 7) / 8;
                
            var value = 0L;
            for (var i = 0; i < byteCount; ++i)
                value |= (long)(_recordData[i + byteOffset] << (8 * i));

            value = (value >> (bitOffset & 7));

            // Prevent possible masking overflows from clamping the actual result.
            if (bitCount != 64)
                value &= ((1L << bitCount) - 1);

            return value;
        }

        public T[] ReadArray<T>(int arraySize, int bitOffset, int bitCount) where T : struct
        {
            var arr = new T[arraySize];
            for (var i = 0; i < arraySize; ++i)
            {
                var itemBitOffset = bitOffset + i * bitCount;
                arr[i] = Read<T>(itemBitOffset);
            }
            return arr;
        }

        public T[] ReadArray<T>(int arraySize) where T : struct
        {
            var nodeSize = SizeCache<T>.Size;

            var arr = new T[arraySize];
            for (var i = 0; i < arraySize; ++i)
            {
                var itemBitOffset = _byteCursor + nodeSize * i;
                arr[i] = Read<T>(itemBitOffset, false);
            }

            _byteCursor += nodeSize * arraySize;
            return arr;
        }

        public string[] ReadStrings(int arraySize, int bitOffset, int bitCount)
        {
            var arr = new string[arraySize];
            for (var i = 0; i < arraySize; ++i)
                arr[i] = ReadString(bitOffset + i * 4, bitCount);
            return arr;
        }

        public string[] ReadStrings(int arraySize)
        {
            var arr = new string[arraySize];
            for (var i = 0; i < arraySize; ++i)
                arr[i] = ReadString();
            return arr;
        }

        public void Dispose()
        {
            _fileReader = null;
            _recordData = null;
        }
    }
}
