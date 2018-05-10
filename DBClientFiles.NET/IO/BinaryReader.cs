using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DBClientFiles.NET.IO
{
    internal class BinaryReader : System.IO.BinaryReader
    {
        private int _bitIndex;

        public BinaryReader(Stream strm, bool keepOpen = false) : base(strm, Encoding.UTF8, keepOpen)
        {
        }

        public override byte[] ReadBytes(int byteCount)
        {
            ResetBitReader();
            return base.ReadBytes(byteCount);
        }

        public void ResetBitReader()
        {
            if (_bitIndex == 0)
                return;

            // Consume the byte and move on
            if (_bitIndex < 8)
                BaseStream.Position += 1;

            _bitIndex = 0;
        }

        public override ulong ReadUInt64()
        {
            ResetBitReader();
            return base.ReadUInt64();
        }

        public override long ReadInt64()
        {
            ResetBitReader();
            return base.ReadInt64();
        }

        public override uint ReadUInt32()
        {
            ResetBitReader();
            return base.ReadUInt32();
        }
        public override int ReadInt32()
        {
            ResetBitReader();
            return base.ReadInt32();
        }

        public virtual uint ReadUInt24()
        {
            ResetBitReader();
            throw new NotImplementedException();
        }
        public virtual int ReadInt24()
        {
            ResetBitReader();

            throw new NotImplementedException();
        }

        public override ushort ReadUInt16()
        {
            ResetBitReader();
            return base.ReadUInt16();
        }
        public override short ReadInt16()
        {
            ResetBitReader();
            return base.ReadInt16();
        }

        public override byte ReadByte()
        {
            ResetBitReader();
            return base.ReadByte();
        }
        public override sbyte ReadSByte()
        {
            ResetBitReader();
            return base.ReadSByte();
        }

        public override unsafe float ReadSingle()
        {
            ResetBitReader();
            return base.ReadSingle();
        }

        public bool ReadBit()
        {
            var currByte = (byte)PeekChar();
            var bitMask = 1 << (7 - _bitIndex++);

            if (_bitIndex == 8)
            {
                BaseStream.Position += 1;
                _bitIndex = 0;
            }

            return (currByte & bitMask) != 0;
        }

        public long ReadBits(int bitCount)
        {
            long value = 0;
            for (var i = bitCount - 1; i >= 0; --i)
                if (ReadBit())
                    value |= 1L << i;

            return value;
        }

        public override string ReadString() => ReadString(Encoding.UTF8);

        public virtual string ReadString(Encoding encoding)
        {
            // At this point the string exceeds the size of the buffer, so just directly read from the stream.
            var byteArray = new List<byte>();
            int charByte;
            while ((charByte = BaseStream.ReadByte()) != 0x00)
            {
                if (charByte == -1)
                    throw new EndOfStreamException();

                byteArray.Add((byte)(charByte & 0xFF));
            }

            return encoding.GetString(byteArray.ToArray());
        }

        internal class Expressions
        {
            public static Dictionary<TypeCode, MethodInfo> Readers = new Dictionary<TypeCode, MethodInfo>()
            {
                { TypeCode.UInt64,  typeof(BinaryReader).GetMethod("ReadUInt64", Type.EmptyTypes) },
                { TypeCode.UInt32,  typeof(BinaryReader).GetMethod("ReadUInt32", Type.EmptyTypes) },
                { TypeCode.UInt16,  typeof(BinaryReader).GetMethod("ReadUInt16", Type.EmptyTypes) },

                { TypeCode.Int64,   typeof(BinaryReader).GetMethod("ReadInt64", Type.EmptyTypes) },
                { TypeCode.Int32,   typeof(BinaryReader).GetMethod("ReadInt32", Type.EmptyTypes) },
                { TypeCode.Int16,   typeof(BinaryReader).GetMethod("ReadInt16", Type.EmptyTypes) },

                { TypeCode.Byte,    typeof(BinaryReader).GetMethod("ReadByte", Type.EmptyTypes) },
                { TypeCode.SByte,   typeof(BinaryReader).GetMethod("ReadSByte", Type.EmptyTypes) },

                { TypeCode.Single,  typeof(BinaryReader).GetMethod("ReadSingle", Type.EmptyTypes) },
                { TypeCode.String,  typeof(BinaryReader).GetMethod("ReadString", Type.EmptyTypes) }
            };
        }
    }
}
