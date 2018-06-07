using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal abstract class BaseFileReader<TKey, TValue> : BaseFileReader<TValue>
        where TKey : struct
        where TValue : class, new()
    {
        private CodeGenerator<TValue, TKey> _codeGenerator;
        public override CodeGenerator<TValue> Generator => _codeGenerator;

        public IndexTableReader IndexTable { get; }

        protected BaseFileReader(IFileHeader header, Stream strm, StorageOptions options) : base(header, strm, options)
        {
            IndexTable = new IndexTableReader(this);
        }

        public override bool PrepareMemberInformations()
        {
            _codeGenerator = new CodeGenerator<TValue, TKey>(this);
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
        protected BaseFileReader(IFileHeader header, Stream strm, StorageOptions options) : base(header, strm, true)
        {
            StringTable = new StringTableSegment(this);
            OffsetMap = new OffsetMapReader(this);
            Records = new Segment();

            Options = options;
        }

        protected override void ReleaseResources()
        {
            _codeGenerator = null;
        }
        #endregion

        #region IStorage implementation
        public uint TableHash { get; protected set; }
        public uint LayoutHash { get; protected set; }
        #endregion

        public ExtendedMemberInfoCollection MemberStore
        {
            get;
            set;
        }

        #region Methods that may be called through deserialization
        // These are called through code generation, don't trust ReSharper.
        public abstract T ReadPalletMember<T>(int memberIndex, RecordReader recordReader, TValue value) where T : struct;
        public abstract T ReadCommonMember<T>(int memberIndex, TValue value) where T : struct;
        public abstract T ReadForeignKeyMember<T>() where T : struct;
        public abstract T[] ReadPalletArrayMember<T>(int memberIndex, RecordReader recordReader, TValue value) where T : struct;
        #endregion
        
        public sealed override StorageOptions Options { get; }

        private CodeGenerator<TValue> _codeGenerator;
        public virtual CodeGenerator<TValue> Generator => _codeGenerator;

        #region Segments
        protected StringTableSegment StringTable;
        protected OffsetMapReader OffsetMap;
        protected Segment Records;
        #endregion

        public event Action<int> OnStringTableEntry;

        public virtual bool PrepareMemberInformations()
        {
            _codeGenerator = new CodeGenerator<TValue>(this);
            return true;
        }
        
        /// <summary>
        /// Enumerates the file, parsing either the records block or the sparse table, if either one exists.
        /// </summary>
        /// <returns></returns>
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
                var recordIndex = 0;
                BaseStream.Seek(Records.StartOffset, SeekOrigin.Begin);

                while (BaseStream.Position < Records.EndOffset)
                {
                    foreach (var node in ReadRecords(recordIndex, BaseStream.Position, Header.RecordSize))
                        yield return node;

                    ++recordIndex;
                }
            }
        }

        /// <summary>
        /// Read any possible amount of records starting at the provided offset and of the given length, including possible copies in the copy table.
        /// </summary>
        /// <see cref="CopyTableReader{TKey}"/>
        /// <param name="recordIndex">The index of this record in the sparse or records block.</param>
        /// <param name="recordOffset">The (absolute) offset in the file at which the record data starts.</param>
        /// <param name="recordSize">The size, in bytes, of the record.</param>
        /// <returns></returns>
        protected abstract IEnumerable<TValue> ReadRecords(int recordIndex, long recordOffset, int recordSize);

        /// <summary>
        /// Populates segment informations
        /// </summary>
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
            return StringTable[tableOffset];
        }

        public U ExtractKey<U>(TValue instance) where U : struct => Generator.ExtractKey<U>(instance);
    }
}
