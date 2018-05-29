using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DBClientFiles.NET.Internals.Serializers;

namespace DBClientFiles.NET.Internals.Versions
{
    internal abstract class BaseFileReader<TKey, TValue> : BaseFileReader<TValue>
        where TKey : struct
        where TValue : class, new()
    {
        private CodeGenerator<TValue, TKey> _codeGenerator;
        public override CodeGenerator<TValue> Generator => _codeGenerator;

        protected BaseFileReader(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
        }

        public override bool ReadHeader()
        {
            _codeGenerator = new CodeGenerator<TValue, TKey>(Members) {
                IsIndexStreamed = !IndexTable.Exists
            };
            return true;
        }

        protected override void ReleaseResources()
        {
            _codeGenerator = null;
        }
    }

    internal abstract class BaseFileReader<TValue> : FileReader, IReader<TValue> where TValue : class, new()
    {
        #region Life and death
        protected BaseFileReader(Stream strm, bool keepOpen) : base(strm, keepOpen)
        {
        }

        protected override void ReleaseResources()
        {
            _codeGenerator = null;
        }
        #endregion

        public uint TableHash { get; protected set; }
        public uint LayoutHash { get; protected set; }

        public virtual ExtendedMemberInfo[] Members { get; protected set; }

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
    
        public virtual Segment<TValue, StringTableReader<TValue>> StringTable => throw new NotImplementedException();
        public virtual Segment<TValue, OffsetMapReader<TValue>> OffsetMap => throw new NotImplementedException();
        public virtual Segment<TValue> Records => throw new NotImplementedException();
        public virtual Segment<TValue> CopyTable => throw new NotImplementedException();
        public virtual Segment<TValue> CommonTable => throw new NotImplementedException();
        public virtual Segment<TValue> IndexTable => throw new NotImplementedException();

        public event Action<long, string> OnStringTableEntry;

        public virtual bool ReadHeader()
        {
            _codeGenerator = new CodeGenerator<TValue>(Members) { IsIndexStreamed = true };
            return true;
        }
        
        public abstract IEnumerable<TValue> ReadRecords();

        public virtual void ReadSegments()
        {
            if (StringTable.Exists)
            {
                if (Options.LoadMask.HasFlag(LoadMask.StringTable))
                    StringTable.Reader.OnStringRead += OnStringTableEntry;

                StringTable.Reader.Read();

                if (Options.LoadMask.HasFlag(LoadMask.StringTable))
                    StringTable.Reader.OnStringRead -= OnStringTableEntry;
            }
        }

        public override string FindStringByOffset(int tableOffset)
        {
            return StringTable.Reader[tableOffset];
        }

        public override string[] ReadStringArray(int[] tableOffsets)
        {
            return tableOffsets.Select(tableOffset => StringTable.Reader[tableOffset]).ToArray();
        }
    }
}
