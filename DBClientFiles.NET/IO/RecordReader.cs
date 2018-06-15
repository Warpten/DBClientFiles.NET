using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.IO
{
    /// <summary>
    /// Metainformation for <see cref="RecordReader"/>.
    /// </summary>
    public static class _RecordReader
    {
        private static Type[] arrayArgs            = { typeof(int) };
        private static Type[] arrayPackedArgs      = { typeof(int), typeof(int), typeof(int) };
        private static Type[] packedArgs           = { typeof(int), typeof(int) };

        public static readonly MethodInfo ReadPackedUInt64  = typeof(RecordReader).GetMethod("ReadUInt64", packedArgs);
        public static readonly MethodInfo ReadPackedUInt32  = typeof(RecordReader).GetMethod("ReadUInt32", packedArgs);
        public static readonly MethodInfo ReadPackedUInt16  = typeof(RecordReader).GetMethod("ReadUInt16", packedArgs);
        public static readonly MethodInfo ReadPackedSByte   = typeof(RecordReader).GetMethod("ReadSByte",  packedArgs);

        public static readonly MethodInfo ReadPackedInt64   = typeof(RecordReader).GetMethod("ReadInt64", packedArgs);
        public static readonly MethodInfo ReadPackedInt32   = typeof(RecordReader).GetMethod("ReadInt32", packedArgs);
        public static readonly MethodInfo ReadPackedInt16   = typeof(RecordReader).GetMethod("ReadInt16", packedArgs);
        public static readonly MethodInfo ReadPackedByte    = typeof(RecordReader).GetMethod("ReadByte",  packedArgs);

        public static readonly MethodInfo ReadPackedSingle  = typeof(RecordReader).GetMethod("ReadSingle", new[] { typeof(int) });

        public static readonly MethodInfo ReadPackedString  = typeof(RecordReader).GetMethod("ReadString", packedArgs);
        public static readonly MethodInfo ReadPackedStrings = typeof(RecordReader).GetMethod("ReadStrings", arrayPackedArgs);
        public static readonly MethodInfo ReadPackedArray   = typeof(RecordReader).GetMethod("ReadArray", arrayPackedArgs);

        public static readonly MethodInfo ReadUInt64        = typeof(RecordReader).GetMethod("ReadUInt64", Type.EmptyTypes);
        public static readonly MethodInfo ReadUInt32        = typeof(RecordReader).GetMethod("ReadUInt32", Type.EmptyTypes);
        public static readonly MethodInfo ReadUInt16        = typeof(RecordReader).GetMethod("ReadUInt16", Type.EmptyTypes);
        public static readonly MethodInfo ReadSByte         = typeof(RecordReader).GetMethod("ReadSByte", Type.EmptyTypes);

        public static readonly MethodInfo ReadInt64         = typeof(RecordReader).GetMethod("ReadInt64", Type.EmptyTypes);
        public static readonly MethodInfo ReadInt32         = typeof(RecordReader).GetMethod("ReadInt32", Type.EmptyTypes);
        public static readonly MethodInfo ReadInt16         = typeof(RecordReader).GetMethod("ReadInt16", Type.EmptyTypes);
        public static readonly MethodInfo ReadByte          = typeof(RecordReader).GetMethod("ReadByte", Type.EmptyTypes);

        public static readonly MethodInfo ReadSingle        = typeof(RecordReader).GetMethod("ReadSingle", Type.EmptyTypes);
        public static readonly MethodInfo ReadString        = typeof(RecordReader).GetMethod("ReadString", Type.EmptyTypes);
        public static readonly MethodInfo ReadStrings       = typeof(RecordReader).GetMethod("ReadStrings", arrayArgs);
        public static readonly MethodInfo ReadArray         = typeof(RecordReader).GetMethod("ReadArray", arrayArgs);

        public static readonly Dictionary<TypeCode, MethodInfo> PackedReaders = new Dictionary<TypeCode, MethodInfo>()
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

        public static Dictionary<TypeCode, MethodInfo> Readers = new Dictionary<TypeCode, MethodInfo>()
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
    internal class RecordReader : IDisposable
    {
        private byte[] _recordData;
        //private GCHandle _dataHandle;
        //private IntPtr _dataPointer;

        protected int _bitCursor {
            get;
            set;
        } = 0;

        public long ReadInt64() => Read<long>(_bitCursor, 64, true);
        public int ReadInt32() => Read<int>(_bitCursor, 32, true);
        public short ReadInt16() => Read<short>(_bitCursor, 16, true);
        public byte ReadByte() => Read<byte>(_bitCursor, 8, true);

        public ulong ReadUInt64() => Read<ulong>(_bitCursor, 64, true);
        public uint ReadUInt32() => Read<uint>(_bitCursor, 32, true);
        public ushort ReadUInt16() => Read<ushort>(_bitCursor, 16, true);
        public sbyte ReadSByte() => Read<sbyte>(_bitCursor, 8, true);

        public float ReadSingle() => Read<float>(_bitCursor, 32, true);

        protected FileReader _fileReader;
        protected readonly bool _usesStringTable;

        public int StartOffset { get; }

        public RecordReader(FileReader fileReader, bool usesStringTable, int recordSize)
        {
            StartOffset = (int)fileReader.BaseStream.Position;

            _usesStringTable = usesStringTable;
            _fileReader = fileReader;

            // See comment block in Read<T> for an in-depth explanation about this.
            // We are only allocating an extra byte because as of now it is extremely unlikely for an int64 to be packed at the end of a record.
            _recordData = new byte[recordSize + 1];
            fileReader.BaseStream.Read(_recordData, 0, recordSize);
        }

        public void Dispose()
        {
            // _dataHandle.Free();

            _fileReader = null;
            _recordData = null;
        }

        public long ReadInt64(int bitOffset, int bitCount)
        {
            if (bitCount <= 32)
                return ReadInt32(bitOffset, bitCount);

            _bitCursor = bitOffset + bitCount;

            var longValue = Read<long>(bitOffset, bitCount) >> (bitOffset & 7);
            if (bitCount != 64)
                longValue &= (1L << bitCount) - 1;

            return longValue;
        }

        public ulong ReadUInt64(int bitOffset, int bitCount)
        {
            if (bitCount <= 32)
                return ReadUInt32(bitOffset, bitCount);

            _bitCursor = bitOffset + bitCount;

            var longValue = Read<ulong>(bitOffset, bitCount) >> (bitOffset & 7);
            if (bitCount != 64)
                longValue &= (1uL << bitCount) - 1;

            return longValue;
        }

        public int ReadInt32(int bitOffset, int bitCount)
        {
            if (bitCount <= 16)
                return ReadInt16(bitOffset, bitCount);

            _bitCursor = bitOffset + bitCount;

            var intValue = Read<int>(bitOffset, bitCount) >> (bitOffset & 7);
            if (bitCount != 32)
                intValue &= (1 << bitCount) - 1;

            return intValue;
        }

        public uint ReadUInt32(int bitOffset, int bitCount)
        {
            if (bitCount <= 16)
                return ReadUInt16(bitOffset, bitCount);

            _bitCursor = bitOffset + bitCount;

            var intValue = Read<uint>(bitOffset, bitCount) >> (bitOffset & 7);
            if (bitCount != 32)
                intValue &= (1u << bitCount) - 1;

            return intValue;
        }

        public short ReadInt16(int bitOffset, int bitCount)
        {
            if (bitCount <= 8)
                return ReadSByte(bitOffset, bitCount);

            _bitCursor = bitOffset + bitCount;

            var shortValue = Read<short>(bitOffset, bitCount) >> (bitOffset & 7);
            if (bitCount != 16)
                shortValue &= (1 << bitCount) - 1;

            return (short)shortValue;
        }

        public ushort ReadUInt16(int bitOffset, int bitCount)
        {
            if (bitCount <= 8)
                return ReadByte(bitOffset, bitCount);

            _bitCursor = bitOffset + bitCount;

            var shortValue = Read<short>(bitOffset, bitCount) >> (bitOffset & 7);
            if (bitCount != 16)
                shortValue &= (1 << bitCount) - 1;

            return (ushort)shortValue;
        }

        public byte ReadByte(int bitOffset, int bitCount)
        {
            _bitCursor = bitOffset + bitCount;

            var byteValue = Read<byte>(bitOffset, bitCount) >> (bitOffset & 7);
            if (bitCount != 8)
                byteValue &= (1 << bitCount) - 1;

            return (byte)byteValue;
        }

        public sbyte ReadSByte(int bitOffset, int bitCount)
        {
            _bitCursor = bitOffset + bitCount;

            var byteValue = Read<sbyte>(bitOffset, bitCount) >> (bitOffset & 7);
            if (bitCount != 8)
                byteValue &= (1 << bitCount) - 1;

            return (sbyte)byteValue;
        }

        public float ReadSingle(int bitOffset)
        {
            return Read<float>(bitOffset, 32);
        }

        /// <remarks>
        /// While this may look fine, it will return a value that will be unaccurate unless properly shifted to the right by <code><paramref name="bitOffset"/> & 7</code>, as this cannot be typically done by this method.
        /// </remarks>
        private T Read<T>(int bitOffset, int bitCount, bool advanceCursor = false) where T : struct
        {
            if (advanceCursor)
                _bitCursor += SizeCache<T>.Size * 8;

            //! Reading whichever is most from SizeCache<T>.Size and (bitCount + (bitOffset & 7) + 7) / 8.
            //! Consider this: Given a field that uses 17 bits, it deserializes as an int32. However, 17 bits fit on 3 bytes, so we would only read three bytes.
            //! MemoryMarshal.Read<int> expects a Span<byte> of Length = 4.
            //! What about reading 33 bits? That deserializes to an int64, so we need 8 bytes instead of 5.
            var spanSlice = _recordData.AsSpan(bitOffset / 8, Math.Max(SizeCache<T>.Size, (bitCount + (bitOffset & 7) + 7) / 8));

            var typeMemory = MemoryMarshal.Read<T>(spanSlice);
            return typeMemory;
        }

        /// <summary>
        /// Reads a string from the record.
        /// </summary>
        /// <returns></returns>
        public virtual string ReadString()
        {
            if (_usesStringTable)
                return _fileReader.FindStringByOffset(ReadInt32());

            return ReadInlineString();
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
            if (bitCount != 32)
                return null;

            if (_usesStringTable)
                return _fileReader.FindStringByOffset(ReadInt32(bitOffset, bitCount));

            if ((bitOffset & 7) == 0)
            {
                _bitCursor = bitOffset;
                return ReadInlineString();
            }

            throw new InvalidOperationException("Packed strings must be in the string block!");
        }

        private string ReadInlineString()
        {
            var byteList = new List<byte>();
            byte currChar;
            while ((currChar = ReadByte()) != '\0')
                byteList.Add(currChar);

            return System.Text.Encoding.UTF8.GetString(byteList.ToArray());
        }

        public long ReadBits(int bitOffset, int bitCount)
        {
            var byteOffset = bitOffset / 8;
            var byteCount = (bitCount + (bitOffset & 7) + 7) / 8;

            var value = 0L;
            for (var i = 0; i < byteCount; ++i)
                value |= (long)(_recordData[i + byteOffset] << (8 * i));

            value = value >> (bitOffset & 7);

            // Prevent possible masking overflows from clamping the actual result.
            if (bitCount != 64)
                value &= ((1L << bitCount) - 1);

            return value;
        }

        public T[] ReadArray<T>(int arraySize, int bitOffset, int bitCount)
            where T : struct
        {
            if (arraySize == 0)
                throw new InvalidOperationException("Trying to read an empty array?");

            var arr = new T[arraySize];
            for (var i = 0; i < arraySize; ++i)
                arr[i] = Read<T>(bitOffset + i * SizeCache<T>.Size * 8, bitCount);
            return arr;
        }

        public T[] ReadArray<T>(int arraySize)
            where T : struct
        {
            var arr = new T[arraySize];
            for (var i = 0; i < arraySize; ++i)
                arr[i] = Read<T>(_bitCursor, SizeCache<T>.Size * 8, true);
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

    }
}
