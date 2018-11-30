using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Parsing.File
{
    /// <summary>
    /// A basic implementation of the <see cref="IParser"/> interface.
    /// </summary>
    /// <typeparam name="TValue">The record type.</typeparam>
    /// <typeparam name="TSerializer"></typeparam>
    internal abstract class BinaryFileParser<TValue, TSerializer> : BinaryReader, IParser<TValue>
        where TSerializer : ISerializer<TValue>, new()
    {
        protected TypeInfo Type { get; }

        /// <summary>
        /// The header.
        /// </summary>
        public abstract ref readonly IFileHeader Header { get; }

        /// <summary>
        /// The options to use for parsing.
        /// </summary>
        public ref readonly StorageOptions Options => ref _options;

        public TSerializer Serializer { get; }

        /// <summary>
        /// The first block of the file (typically its header).
        /// </summary>
        protected Block Head { get; set; }


        private StorageOptions _options;
        private BlockHandlers _handlers;

        // This is a guard against trying to enumerate multiple times. We parsed
        // most of the file on the first run and now all our data structures
        // are in memory
        private bool _prepared = false; // You are not prepared!

        /// <summary>
        /// Create an instance of <see cref="BinaryFileParser{TValue, TSerializer}"/>.
        /// </summary>
        /// <param name="options">The options to use for parsing.</param>
        /// <param name="input">The input stream.</param>
        /// <param name="leaveOpen">If <c>true</c>, the stream is left open once this object is disposed.</param>
        protected BinaryFileParser(in StorageOptions options, Stream input) : base(input, Encoding.UTF8, true)
        {
            if (!input.CanSeek)
                throw new ArgumentException("The stream provided to DBClientFiles.NET's collections has to be seekable!");

            Type = new TypeInfo(typeof(TValue));
            _options = options;

            Serializer = new TSerializer();
            Serializer.Initialize(Type, in Options);
        }

        public void Initialize()
        {
            Head = new Block {
                Identifier = BlockIdentifier.Header,
                Length = Header.Size + 4
            };
        }

        protected override void Dispose(bool disposing)
        {
            Head = default;
            _handlers.Clear();

            base.Dispose(disposing);
        }

        public Block FindBlock(BlockIdentifier identifier)
        {
            var itr = Head;
            while (itr != null && itr.Identifier != identifier)
                itr = itr.Next;
            return itr;
        }

        public U FindBlockHandler<U>(BlockIdentifier identifier) where U : IBlockHandler
            => _handlers.GetHandler<U>(identifier);

        public void RegisterBlockHandler<U>() where U : IBlockHandler, new()
            => RegisterBlockHandler(new U());
        public void RegisterBlockHandler(IBlockHandler handler)
            => _handlers.Register(handler);

        /// <summary>
        /// Superclasses need implement this method to set up the linked chain of file blocks.
        /// </summary>
        protected abstract void Prepare();

        private void DoPrepare()
        {
            if (_prepared)
                return;

            _prepared = true;
            Prepare();
        }

        public abstract BaseMemberMetadata GetFileMemberMetadata(int index);

        /// <summary>
        /// Obtains an instance of <see cref="IRecordReader"/> used for record reading.
        /// </summary>
        /// <returns></returns>
        protected abstract IRecordReader GetRecordReader();

        public class Enumerator : IEnumerator<TValue>, IEnumerator
        {
            internal BinaryFileParser<TValue, TSerializer> _owner;

            internal int _itr;
            internal IndexTableHandler _indexTableHandler;

            internal Block _recordBlock;

            internal TSerializer Serializer => _owner.Serializer;

            public Enumerator(BinaryFileParser<TValue, TSerializer> owner)
            {
                _owner = owner;

                if (owner._prepared)
                {
                    _recordBlock = owner.FindBlock(BlockIdentifier.Records);
                }
                else
                {
                    owner.Prepare();
                    owner.BaseStream.Position = 0;

                    // This is annoying but the compiler *really* wants us to initialize all
                    // members.
                    _recordBlock = null;

                    var head = owner.Head;
                    while (head != null)
                    {
                        if (!owner._handlers.ReadBlock(owner, head))
                            owner.BaseStream.Seek(head.Length, SeekOrigin.Current);

                        if (head.Identifier == BlockIdentifier.Records)
                            _recordBlock = head;

                        head = head.Next;
                    }

                }

                _indexTableHandler = owner._handlers.GetHandler<IndexTableHandler>(BlockIdentifier.IndexTable);
                _itr = 0;

                _current = default;
                Reset();
            }

            internal TValue _current;

            public TValue Current => _current;

            object IEnumerator.Current => Current;

            public virtual void Dispose()
            {
                _recordBlock = null;
                _owner = null;
            }

            public virtual bool MoveNext()
            {
                if (_owner.BaseStream.Position >= _recordBlock.EndOffset)
                    return false;

                _current = _owner.Serializer.Deserialize(_owner.GetRecordReader());
                if (_owner.Header.HasIndexTable)
                    _owner.Serializer.SetKey(out _current, _indexTableHandler[_itr++]);

                return true;
            }

            public void Reset()
            {
                if (_recordBlock != null && _recordBlock.Length != 0)
                    _owner.BaseStream.Position = _recordBlock.StartOffset;

                _current = default;
            }
        }

        public class CopyTableEnumerator : Enumerator
        {
            internal CopyTableHandler _copyTableHandler;
            internal List<int> _currentSourceKey;
            internal int _idxTargetKey;

            public CopyTableEnumerator(BinaryFileParser<TValue, TSerializer> owner, CopyTableHandler copyTableHandler) : base(owner)
            {
                _copyTableHandler = copyTableHandler;

                _currentSourceKey = default;
                _idxTargetKey = 0;
            }

            public override void Dispose()
            {
                base.Dispose();
                _copyTableHandler = null;
            }

            public override bool MoveNext()
            {
                if (_currentSourceKey == null || _idxTargetKey == _currentSourceKey.Count)
                {
                    var success = base.MoveNext();
                    if (!success)
                        return false;

                    _currentSourceKey = _copyTableHandler[Serializer.GetKey(in _current)].ToList();
                    _idxTargetKey = 0;
                    return true;
                }

                var newCurrent = Serializer.Clone(in _current);
                Serializer.SetKey(out newCurrent, _currentSourceKey[_idxTargetKey]);
                ++_idxTargetKey;

                _current = newCurrent;

                return true;
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            var copyTableHandler = _handlers.GetHandler<CopyTableHandler>(BlockIdentifier.CopyTable);
            if (copyTableHandler != null)
                return new CopyTableEnumerator(this, copyTableHandler);

            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var copyTableHandler = _handlers.GetHandler<CopyTableHandler>(BlockIdentifier.CopyTable);
            if (copyTableHandler != null)
                return new CopyTableEnumerator(this, copyTableHandler);

            return new Enumerator(this);
        }

        public abstract int RecordCount { get; }
    }
}
