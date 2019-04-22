using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    internal class Enumerator<TValue, TSerializer> : IEnumerator<TValue>, IEnumerator
        where TSerializer : ISerializer<TValue>, new()
    {
        internal BinaryFileParser<TValue, TSerializer> _owner;

        internal int _itr;

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

            Debug.Assert(_recordBlock != null, "A record block is required in WDBC!");

            owner.After(ParsingStep.Segments);

            // Segments have been processed, it's now time to initialize the deserializer.
            Serializer.Initialize(owner);

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
            if (_owner.BaseStream.Position >= _recordBlock.EndOffset)
                return false;

            IRecordReader recordReader = _owner.GetRecordReader(_owner.Header.RecordSize);
            _current = _owner.Serializer.Deserialize(recordReader, _owner);
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
