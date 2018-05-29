using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Internals.Serializers;

namespace DBClientFiles.NET.Internals.Versions
{
    internal abstract class BaseFileReader<TKey, TValue> : BaseFileReader<TValue>
        where TKey : struct
        where TValue : class, new()
    {
        private CodeGenerator<TValue, TKey> _codeGenerator;
        public override CodeGenerator<TValue> Generator => _codeGenerator;

        public IndexTableReader<TKey, TValue> IndexTable { get; }

        protected BaseFileReader(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
            IndexTable = new IndexTableReader<TKey, TValue>(this);
        }

        public override bool ReadHeader()
        {
            _codeGenerator = new CodeGenerator<TValue, TKey>(Members) {
                IsIndexStreamed = !IndexTable.Exists
            };
            return true;
        }

        public override void ReadSegments()
        {
            base.ReadSegments();

            IndexTable.Read();
        }

        protected override void ReleaseResources()
        {
            _codeGenerator = null;

            IndexTable.Dispose();
        }
    }

    internal abstract class BaseFileReader<TValue> : FileReader, IReader<TValue> where TValue : class, new()
    {
        #region Life and death
        protected BaseFileReader(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
            StringTable = new StringTableSegment<TValue>(this);
            OffsetMap = new OffsetMapReader<TValue>(this);
            Records = new Segment<TValue>();
        }

        protected override void ReleaseResources()
        {
            _codeGenerator = null;
        }
        #endregion
        
        public uint TableHash { get; protected set; }
        public uint LayoutHash { get; protected set; }

        public virtual ExtendedMemberInfo[] Members { get; protected set; }

        // These are called through code generation, don't trust ReSharper.
        public abstract T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value) where T : struct;
        public abstract T ReadCommonMember<T>(int memberIndex, RecordReader recordReader, TValue value) where T : struct;
        public abstract T ReadForeignKeyMember<T>() where T : struct;
        public abstract T[] ReadPalletArrayMember<T>(int memberIndex, RecordReader recordReader, TValue value) where T : struct;

        private StorageOptions _options;
        public override StorageOptions Options
        {
            get => _options;
            set
            {
                var oldOptions = _options;
                _options = value;

                if (oldOptions == null || oldOptions.MemberType != _options.MemberType)
                    Members = typeof(TValue).GetMemberInfos(_options);
            }
        }

        private CodeGenerator<TValue> _codeGenerator;
        public virtual CodeGenerator<TValue> Generator => _codeGenerator;

        #region Segments
        protected StringTableSegment<TValue> StringTable;
        protected OffsetMapReader<TValue> OffsetMap;
        protected Segment<TValue> Records;
        #endregion

        public event Action<long, string> OnStringTableEntry;

        public virtual bool ReadHeader()
        {
            _codeGenerator = new CodeGenerator<TValue>(Members) { IsIndexStreamed = true };
            return true;
        }
        
        public virtual IEnumerable<TValue> ReadRecords()
        {
            if (OffsetMap.Exists)
            {
                for (var i = 0; i < OffsetMap.Count; ++i)
                {
                    foreach (var node in ReadRecords(i, OffsetMap.GetRecordOffset(i), OffsetMap.GetRecordSize(i)))
                        yield return node;
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(Records.ItemLength != 0, "An implementation forgot to set Records.ItemLength");

                var recordIndex = 0;
                BaseStream.Seek(Records.StartOffset, SeekOrigin.Begin);

                while (BaseStream.Position < Records.EndOffset)
                {
                    foreach (var node in ReadRecords(recordIndex, BaseStream.Position, Records.ItemLength))
                        yield return node;

                    ++recordIndex;
                }
            }
        }

        protected abstract IEnumerable<TValue> ReadRecords(int recordIndex, long recordOffset, int recordSize);

        public virtual void ReadSegments()
        {
            if (StringTable.Segment.Exists)
            {
                if (Options.LoadMask.HasFlag(LoadMask.StringTable))
                    StringTable.OnStringRead += OnStringTableEntry;

                StringTable.Read();

                if (Options.LoadMask.HasFlag(LoadMask.StringTable))
                    StringTable.OnStringRead -= OnStringTableEntry;
            }
        }

        public override string FindStringByOffset(int tableOffset)
        {
            var oldPosition = BaseStream.Position;
            BaseStream.Seek(tableOffset + StringTable.StartOffset, SeekOrigin.Begin);
            var str = Options.InternStrings ? string.Intern(ReadString()) : ReadString();
            BaseStream.Seek(oldPosition, SeekOrigin.Begin);
            return str;
        }
    }
}
