using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BinaryReader = DBClientFiles.NET.IO.BinaryReader;

namespace DBClientFiles.NET.Internals.Versions
{
    internal abstract class BaseReader<TKey, TValue> : BaseReader<TValue> where TKey : struct where TValue : class, new()
    {
        protected BaseReader(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
        }

        public override void ReadSegments()
        {
            if (CopyTable.Exists && !CopyTable.Deserialized)
            {
                CopyTable = new Segment<TValue, CopyTableReader<TKey, TValue>>(this);
            }

            base.ReadSegments();
        }
    }

    internal abstract class BaseReader<TValue> : BinaryReader, IReader<TValue> where TValue : class, new()
    {
        protected BaseReader(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
            StringTable = new Segment<TValue, StringTableReader<TValue>>(this);
            OffsetMap = new Segment<TValue, OffsetMapReader<TValue>>(this);
            Records = new Segment<TValue>(this);
            CommonTable = new Segment<TValue>(this);
            CopyTable = new Segment<TValue>(this);
            IndexTable = new Segment<TValue, IndexTableReader<TValue>>(this);
        }

        public int FieldCount { get; protected set; }
        public ExtendedMemberInfo[] ValueMembers { get; protected set; }

        public Type ValueType { get; } = typeof(TValue);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            StringTable.Dispose();
            OffsetMap.Dispose();
            Records.Dispose();
            CopyTable.Dispose();
            IndexTable.Dispose();
            CommonTable.Dispose();
        }

        private StorageOptions _options;
        public StorageOptions Options
        {
            get => _options;
            set
            {
                var oldOptions = _options;
                _options = value;

                if (oldOptions == null || oldOptions.MemberType != _options.MemberType)
                {
                    var members = typeof(TValue).GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(m => m.MemberType == _options.MemberType).ToArray();
                    ValueMembers = new ExtendedMemberInfo[members.Length];
                    for (var i = 0; i < members.Length; ++i)
                        ValueMembers[i] = ExtendedMemberInfo.Initialize(members[i], i);
                }
            }
        }

        public virtual Segment<TValue, StringTableReader<TValue>> StringTable { get; protected set; }
        public virtual Segment<TValue, OffsetMapReader<TValue>> OffsetMap { get; protected set; }

        public virtual Segment<TValue> Records { get; protected set; }

        public virtual Segment<TValue> CopyTable { get; protected set; }

        public virtual Segment<TValue> CommonTable { get; protected set; }
        public virtual Segment<TValue, IndexTableReader<TValue>> IndexTable { get; protected set; }

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

            if (IndexTable.Exists && !IndexTable.Deserialized)
                IndexTable.Reader.Read();
        }

        public override string ReadString()
        {
            if (StringTable.Exists)
            {
                var offset = ReadInt32();
                return StringTable.Reader[ReadInt32()];
            }

            return ReadStringDirect();
        }

        internal string ReadStringDirect()
        {
            var byteList = new List<byte>();
            byte currChar;
            while ((currChar = ReadByte()) != '\0')
                byteList.Add(currChar);

            return Encoding.UTF8.GetString(byteList.ToArray());
        }
    }
}
