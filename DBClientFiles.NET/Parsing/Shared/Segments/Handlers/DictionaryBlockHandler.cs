using DBClientFiles.NET.Parsing.Versions;
using System;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers
{
    internal abstract class DictionaryBlockHandler<TKey, TValue> : ISegmentHandler
        where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _store = new Dictionary<TKey, TValue>();

        public void ReadSegment(IBinaryStorageFile reader, long startOffset, long length)
        {
            if (length == 0)
                return;

            reader.DataStream.Position = startOffset;

            while (reader.DataStream.Position <= (startOffset + length))
                ReadPair(reader.DataStream);
        }

        protected virtual void ReadPair(Stream reader)
        {
            var key = ReadKey(reader);
            _store[key] = ReadValue(reader, key);
        }

        public void WriteSegment<T, U>(T reader) where T : BinaryWriter, IWriter<U>
        {
            throw new NotImplementedException();
        }

        protected abstract TKey ReadKey(Stream reader);
        protected abstract TValue ReadValue(Stream reader, TKey key);

        public abstract void WriteKey(BinaryWriter writer, TKey key);
        public abstract void WriteValue(BinaryWriter writer, TValue value);

        public TValue this[TKey key] => _store.TryGetValue(key, out var value) ? value : default;

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _store.TryGetValue(key, out value);
        }

        protected void Add(TKey key, TValue value)
        {
            _store[key] = value;
        }
    }
}
