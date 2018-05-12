using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

        public int FieldCount { get; protected set; }
        public ExtendedMemberInfo[] ValueMembers { get; protected set; }

        public Type ValueType { get; } = typeof(TValue);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private StorageOptions _options;
        public StorageOptions Options
        {
            get => _options;
            set
            {
                var oldOptions = _options;
                _options = value;

                if (oldOptions.MemberType != _options.MemberType)
                {
                    var members = typeof(TValue).GetMembers(BindingFlags.Public | BindingFlags.Instance);
                    ValueMembers = new ExtendedMemberInfo[members.Length];
                    for (var i = 0; i < members.Length; ++i)
                        ValueMembers[i] = ExtendedMemberInfo.Initialize(members[i], i);
                }
            }
        }

        public Segment<TValue, StringTableReader<TValue>> StringTable { get; private set; }
        public Segment<TValue, OffsetMapReader<TValue>> OffsetMap { get; private set; }

        public Segment<TValue> Records { get; private set; }

        public Segment<TValue, CopyTableReader<TValue>> CopyTable { get; private set; }

        public Segment<TValue> CommonTable { get; protected set; }
        public Segment<TValue, IndexTableReader<TValue>> IndexTable { get; private set; }

        public event Action<long, string> OnStringTableEntry;

        public abstract bool ReadHeader();

        public abstract IEnumerable<TValue> ReadRecords();

        public virtual void ReadSegments()
        {
            if (Options.LoadMask.HasFlag(LoadMask.StringTable) && StringTable.Exists)
            {
                StringTable.Reader.OnStringRead += OnStringTableEntry;
                StringTable.Reader.Read();
                StringTable.Reader.OnStringRead -= OnStringTableEntry;
            }

            if (OffsetMap.Exists)
            {
                OffsetMap.Reader.MinIndex = Header.MinIndex;
                OffsetMap.Reader.MaxIndex = Header.MaxIndex;
                OffsetMap.Reader.Read();
            }

            if (CopyTable.Exists)
                CopyTable.Reader.Read();

            if (IndexTable.Exists)
                IndexTable.Reader.Read();
        }

        public override string ReadString()
        {
            if (StringTable.Exists)
            {
                var offset = ReadInt32();
                return StringTable.Reader[ReadInt32()];
            }

            return base.ReadString();
        }

        internal string ReadStringDirect() => base.ReadString();
    }
}
