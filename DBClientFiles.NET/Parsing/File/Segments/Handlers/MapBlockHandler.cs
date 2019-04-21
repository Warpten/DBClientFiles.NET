using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal abstract class MapBlockHandler<TKey, TValue> : IBlockHandler
    {
        public abstract BlockIdentifier Identifier { get; }

        private Dictionary<TKey, TValue> _store = new Dictionary<TKey, TValue>();

        public void ReadBlock(BinaryReader reader, long startOffset, long length)
        {
            if (length == 0)
                return;

            Debug.Assert(reader.BaseStream.Position == startOffset, "Out-of-place parsing!");

            while (reader.BaseStream.Position <= (startOffset + length))
                ReadPair(reader);
        }

        protected virtual void ReadPair(BinaryReader reader)
        {
            var key = ReadKey(reader);
            _store[key] = ReadValue(reader, key);
        }

        public void WriteBlock<T, U>(T reader) where T : BinaryWriter, IWriter<U>
        {
            throw new NotImplementedException();
        }

        public abstract TKey ReadKey(BinaryReader reader);
        public abstract TValue ReadValue(BinaryReader reader, TKey key);

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
