using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.IO
{
    public static class _RecordReader
    {
        private static Type[] argTypes { get; } = new[] { typeof(int), typeof(int) };

        public static MethodInfo ReadUInt64 { get; } = typeof(RecordReader).GetMethod("ReadUInt64", argTypes);
        public static MethodInfo ReadUInt32 { get; } = typeof(RecordReader).GetMethod("ReadUInt32", argTypes);
        public static MethodInfo ReadUInt16 { get; } = typeof(RecordReader).GetMethod("ReadUInt16", argTypes);
        public static MethodInfo ReadSByte  { get; } = typeof(RecordReader).GetMethod("ReadSByte",  argTypes);

        public static MethodInfo ReadInt64  { get; } = typeof(RecordReader).GetMethod("ReadInt64", argTypes);
        public static MethodInfo ReadInt32  { get; } = typeof(RecordReader).GetMethod("ReadInt32", argTypes);
        public static MethodInfo ReadInt16  { get; } = typeof(RecordReader).GetMethod("ReadInt16", argTypes);
        public static MethodInfo ReadByte   { get; } = typeof(RecordReader).GetMethod("ReadByte",  argTypes);
        
        public static MethodInfo ReadSingle { get; } = typeof(RecordReader).GetMethod("ReadSingle", new[] { typeof(int) });

        public static MethodInfo ReadArray { get; } = typeof(RecordReader).GetMethod("ReadArray");
    }

    internal sealed unsafe class RecordReader : IDisposable
    {
        private byte[] _recordData;
        private int _byteCursor = 0;


        public RecordReader(Stream input, int recordSize)
        {
            using (var reader = new BinaryReader(input, Encoding.UTF8, true))
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

        private T Read<T>(int bitOffset) where T : unmanaged
        {
            T v;
            fixed (byte* b = _recordData)
            {
                var data = b + bitOffset / 8;
                v = *(T*)&data[0];
            }
            return v;
        }

        public string ReadString(int bitOffset, int bitCount)
        {
            var byteList = new List<byte>();
            byte currChar;
            while ((currChar = ReadByte()) != '\0')
                byteList.Add(currChar);

            return Encoding.UTF8.GetString(byteList.ToArray());
        }

        private byte ReadByte()
        {
            var currentNode = _recordData[_byteCursor / 8];
            _byteCursor += 8;
            return currentNode;
        }
        
        public long ReadBits(int bitOffset, int bitCount)
        {
            // This is how the client reads stuff
            //    var value = 0b0110 1010 1111 0000;
            // Given field_size_bits = 13
            // and   field_offset_bits = 0
            //    var x = (field_size_bits + (field_offset_bits & 7) + 7) / 8 = 2
            // Bytes are read little-endian, meaning we get
            //    var value = 0b1111 0000 0110 1010
            // And then we shift and mask:
            //    value = (value >> (field_offset_bits & 7)) & ((1ull << field_size_bits) - 1)) = 4202 = 0b1 0000 0110 1010
            
            var byteOffset = bitOffset / 8;
            var byteCount = (bitCount + (bitOffset & 7) + 7) / 8;
                
            var value = 0L;
            for (var i = 0; i < byteCount; ++i)
                value |= (long)(_recordData[i + byteOffset] << (8 * i));

            value = (value >> (bitOffset & 7));
            if (bitCount != 64)
                value &= ((1L << bitCount) - 1);

            return value;
        }

        public T[] ReadArray<T>(int arraySize, int bitOffset, int bitCount) where T : unmanaged
        {
            var arr = new T[arraySize];
            for (var i = 0; i < arraySize; ++i)
            {
                var itemBitOffset = bitOffset + i * bitCount;
                arr[i] = Read<T>(itemBitOffset);
            }
            return arr;
        }

        public void Dispose()
        {
            _recordData = null;
        }
    }
}
