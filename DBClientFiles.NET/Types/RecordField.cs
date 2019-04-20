using DBClientFiles.NET.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Types
{
    /// <summary>
    /// This is a representation of a field where size and type are unknown.
    /// </summary>
    public unsafe class RecordField
    {
        private int _recordOffset;
        private int _bitCount;

        internal RecordField(int offset, int bitCount, Record record)
        {
            _recordOffset = offset;
            _bitCount = bitCount;

            Record = record;
        }

        internal RecordField(int offset, int bitCount, int cardinality, Record record) : this(offset, bitCount, record)
        {
            // _cardinality = cardinality;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T Read<T>() where T : unmanaged
        {
            var byteOffset = _recordOffset / 8;
            var bitOffset = _recordOffset & 7;

            var dataPtr = (byte*) Unsafe.AsPointer(ref Record.Data[byteOffset]);
            return *(T*)dataPtr;
        }

        public string Name { get; }
        public Record Record { get; }

        public ulong UInt64 => Read<ulong>();
        public uint UInt32 => Read<uint>();
        public ushort UInt16 => Read<ushort>();
        public byte UInt8 => Read<byte>();

        public long Int64 => Read<long>();
        public int Int32 => Read<int>();
        public short Int16 => Read<short>();
        public sbyte Int8 => Read<sbyte>();

        public float Single => Read<float>();
        public double Double => Read<double>();

        public string String => throw new NotImplementedException();
    }

    /// <summary>
    /// This is a representation of a record made of a varying amount of members, unknown at compile time.
    /// </summary>
    public sealed class Record
    {
        /// <summary>
        /// The backing container.
        /// </summary>
        internal StorageList List { get; }

        /// <summary>
        /// The record's underlying data.
        /// </summary>
        internal byte[] Data { get; }

        /// <summary>
        /// A collection of members of the record, keyed by their name
        /// </summary>
        internal IDictionary<String, RecordField> Members { get; }

        public RecordField this[string memberName]
        {
            get { return Members[memberName]; }
        }
    }
}
