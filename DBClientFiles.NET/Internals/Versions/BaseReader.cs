using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using BinaryReader = DBClientFiles.NET.IO.BinaryReader;

namespace DBClientFiles.NET.Internals.Versions
{
    internal abstract class BaseReader<TKey, TValue> : BaseReader<TValue> where TKey : struct where TValue : class, new()
    {
        protected BaseReader(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
        }
    }

    internal abstract class BaseReader<TValue> : BinaryReader, IReader<TValue> where TValue : class, new()
    {
        protected BaseReader(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
        }

        public BaseReader() : base(null)
        {

        }

        public int FieldCount { get; protected set; }

        public virtual Type ValueType { get; } = typeof(TValue);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public StorageOptions Options { get; set; }

        public Segment<TValue> StringTable { get; private set; }
        private StringTableReader<TValue> StringTableReader { get; set; }

        public Segment<TValue> OffsetMap { get; private set; }
        public Segment<TValue> Records { get; private set; }
        public Segment<TValue> CopyTable { get; private set; }
        public Segment<TValue> CommonTable { get; protected set; }
        public Segment<TValue> IndexTable { get; private set; }

        public event Action<long, string> OnStringTableEntry;

        public abstract bool ReadHeader();

        public abstract IEnumerable<TValue> ReadRecords();

        public virtual void ReadSegments()
        {
            if (Options.LoadMask.HasFlag(LoadMask.StringTable) && StringTable.Exists)
            {
                StringTableReader = new StringTableReader<TValue>(StringTable);
                StringTableReader.OnStringRead += OnStringTableEntry;
                StringTableReader.Read();
                StringTableReader.OnStringRead -= OnStringTableEntry;
            }

            if (OffsetMap.Exists)
            {
                var offsetmapReader = new OffsetmapReader<TValue>(OffsetMap);
                offsetmapReader.MinIndex = Header.MinIndex;
                offsetmapReader.MaxIndex = Header.MaxIndex;
                offsetmapReader.Read();
            }

            if (CommonTable.Exists)
                CommonTable.Read(this);

            if (IndexTable.Exists)
                IndexTable.Read(this);
        }

        public override string ReadString()
        {
            if (StringTable.Exists)
            {
                var offset = ReadInt32();
                return StringTableReader[(int)(ReadInt32() + StringTable.StartOffset)];
            }

            return base.ReadString();
        }

        internal string ReadStringDirect() => base.ReadString();

        public override unsafe float ReadSingle()
        {
            int intValue = ReadInt32();
            return *(float*)&intValue;
        }
    }
}
