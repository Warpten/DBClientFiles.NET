using DBClientFiles.NET.Collections;
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
                    ValueMembers = typeof(TValue).GetMemberInfos(_options);
            }
        }

        public virtual Segment<TValue, StringTableReader<TValue>> StringTable { get; }
        public virtual Segment<TValue, OffsetMapReader<TValue>> OffsetMap { get; }

        public virtual Segment<TValue> Records { get; }

        public virtual Segment<TValue> CopyTable { get; }

        public virtual Segment<TValue> CommonTable { get; }
        public virtual Segment<TValue, IndexTableReader<TValue>> IndexTable { get; }

        public event Action<long, string> OnStringTableEntry;

        public abstract bool ReadHeader();

        public abstract IEnumerable<TValue> ReadRecords();

#if PERFORMANCE
        public TimeSpan CloneGeneration { get; protected set; }
        public TimeSpan DeserializeGeneration { get; protected set; }
#endif


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
                return StringTable.Reader[ReadInt32()];

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
