using System.Collections;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.File
{
    /// <summary>
    /// An implementation of the enumerator used to read the records.
    /// </summary>
    internal class Enumerator<TValue, TSerializer> : IEnumerator<TValue>, IEnumerator
        where TSerializer : ISerializer<TValue>, new()
    {
        internal BinaryFileParser<TValue, TSerializer> _owner;

        internal int _itr;
        internal IndexTableHandler _indexTableHandler;
        internal OffsetMapHandler _offsetMapHandler;

        internal Block _recordBlock;

        internal TSerializer Serializer => _owner.Serializer;

        public Enumerator(BinaryFileParser<TValue, TSerializer> owner)
        {
            _owner = owner;
            owner.BaseStream.Position = 0;

            owner.Before(ParsingStep.Segments);

            var head = owner.Head;
            while (head != null)
            {
                if (!head.ReadBlock(owner))
                    owner.BaseStream.Seek(head.Length, SeekOrigin.Current);
            
                // While at it, we store the records block, since that one is unique.
                if (head.Identifier == BlockIdentifier.Records)
                    _recordBlock = head;

                head = head.Next;
            }

            owner.After(ParsingStep.Segments);

            // Segments have been processed, it's now time to initialize the deserializer.
            Serializer.Initialize(owner);

            // Try to retrieve commonly found block handlers for use when enumerating.
            _offsetMapHandler = owner.FindBlock(BlockIdentifier.OffsetMap)?.Handler as OffsetMapHandler;
            _indexTableHandler = owner.FindBlock(BlockIdentifier.IndexTable)?.Handler as IndexTableHandler;
            _itr = 0;

            _current = default;
            Reset();
        }

        //! Not an auto-property because the key assignment takes an out parameter.
        //! TODO: Does it *really* need to be an out parameter?
        internal TValue _current;
        public TValue Current => _current;

        // Explicit implementation of the non-generic IEnumerator interface.
        object IEnumerator.Current => Current;

        public virtual void Dispose()
        {
            _recordBlock = null;
            _owner = null;
        }

        public virtual bool MoveNext()
        {
            IRecordReader recordReader;
            if (_recordBlock != null)
            {
                if (_owner.BaseStream.Position >= _recordBlock.EndOffset)
                    return false;

                recordReader = _owner.GetRecordReader(_owner.Header.RecordSize);
            }
            else // if (_offsetMapHandler != null) // Implied
            {
                _owner.BaseStream.Seek(_offsetMapHandler.GetRecordOffset(_itr), SeekOrigin.Begin);

                recordReader = _owner.GetRecordReader(_offsetMapHandler.GetRecordSize(_itr));
            }

            _current = _owner.Serializer.Deserialize(recordReader, _owner);

            if (_owner.Header.HasIndexTable)
                _owner.Serializer.SetKey(out _current, _indexTableHandler[_itr++]);

            return true;
        }

        public virtual void Reset()
        {
            if (_recordBlock != null && _recordBlock.Length != 0)
                _owner.BaseStream.Position = _recordBlock.StartOffset;

            _itr = 0;
            _current = default;
        }
    }
}
