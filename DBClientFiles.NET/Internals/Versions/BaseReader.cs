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

    internal abstract class BaseReader<TValue> : BaseReader, IReader<TValue> where TValue : class, new()
    {
        protected BaseReader(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
        }

        public int FieldCount { get; protected set; }

        public sealed override Type ValueType { get; } = typeof(TValue);

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

        public Segment<TValue> StringTable { get; private set; }
        private StringTableReader<TValue> StringTableReader { get; set; }

        public Segment<TValue> OffsetMap { get; private set; }
        private OffsetmapReader<TValue> offsetmapReader { get; set; }

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
                offsetmapReader = new OffsetmapReader<TValue>(OffsetMap);
                offsetmapReader.MinIndex = Header.MinIndex;
                offsetmapReader.MaxIndex = Header.MaxIndex;
                offsetmapReader.Read();
            }
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
    }

    internal abstract class BaseReader : BinaryReader
    {
        public virtual Type ValueType { get; } = typeof(object);

        public ExtendedMemberInfo[] ValueMembers { get; protected set; }

        protected BaseReader(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
        }

        public override unsafe float ReadSingle()
        {
            int intValue = ReadInt32();
            return *(float*)&intValue;
        }
    }
}
