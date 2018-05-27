﻿using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DBClientFiles.NET.Internals.Versions
{
    internal abstract class BaseFileReader<TKey, TValue> : BaseFileReader<TValue> where TKey : struct where TValue : class, new()
    {
        protected BaseFileReader(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
        }
    }

    internal abstract class BaseFileReader<TValue> : FileReader, IReader<TValue> where TValue : class, new()
    {
        protected BaseFileReader(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
        }

        public int FieldCount { get; protected set; }
        public ExtendedMemberInfo[] ValueMembers { get; protected set; }

        public Type ValueType { get; } = typeof(TValue);

        public abstract T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value);
        public abstract T ReadCommonMember<T>(int memberIndex, RecordReader recordReader, TValue value);
        public abstract T ReadForeignKeyMember<T>(int memberIndex, RecordReader recordReader, TValue value);
        public abstract T[] ReadPalletArrayMember<T>(int memberIndex, RecordReader recordReader, TValue value);

        private StorageOptions _options;
        public StorageOptions Options
        {
            get => _options;
            set
            {
                var oldOptions = _options;
                _options = value;

                if (oldOptions == null || oldOptions.MemberType != _options.MemberType)
                    ValueMembers = typeof(TValue).GetMemberInfos(_options);
            }
        }

        public virtual Segment<TValue, StringTableReader<TValue>> StringTable { get { throw new NotImplementedException(); } }
        public virtual Segment<TValue, OffsetMapReader<TValue>> OffsetMap { get { throw new NotImplementedException(); } }
        public virtual Segment<TValue> Records { get { throw new NotImplementedException(); } }
        public virtual Segment<TValue> CopyTable { get { throw new NotImplementedException(); } }
        public virtual Segment<TValue> CommonTable { get { throw new NotImplementedException(); } }
        public virtual Segment<TValue> IndexTable { get { throw new NotImplementedException(); } }

        public event Action<long, string> OnStringTableEntry;

        public abstract bool ReadHeader();

        public abstract IEnumerable<TValue> ReadRecords();

        public virtual void ReadSegments()
        {
            if (StringTable.Exists && !StringTable.Deserialized)
            {
                if (Options.LoadMask.HasFlag(LoadMask.StringTable))
                    StringTable.Reader.OnStringRead += OnStringTableEntry;

                StringTable.Reader.Read();

                if (Options.LoadMask.HasFlag(LoadMask.StringTable))
                    StringTable.Reader.OnStringRead -= OnStringTableEntry;
            }

            // This is shoddy design (It relies on execution flow when calling base method in children) but meh. Let's keep it for safety purposes.
            if (OffsetMap.Exists && !OffsetMap.Deserialized)
                throw new InvalidOperationException("Offset map needs to be deserialized in children classes!");
        }

        public override string ReadString(int tableOffset)
        {
            return StringTable.Reader[tableOffset];
        }
    }
}
