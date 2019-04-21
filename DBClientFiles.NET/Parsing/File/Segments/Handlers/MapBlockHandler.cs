using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal abstract class MapBlockHandler<TKey, TValue> : IBlockHandler
    {
        private Dictionary<TKey, TValue> _store = new Dictionary<TKey, TValue>();

        public void ReadBlock(IBinaryStorageFile reader, long startOffset, long length)
        {
            if (length == 0)
                return;

            Debug.Assert(reader.BaseStream.Position == startOffset, "Out-of-place parsing!");

            using (var streamReader = new BinaryReader(reader.BaseStream, Encoding.UTF8, true))
                while (reader.BaseStream.Position <= (startOffset + length))
                    ReadPair(streamReader);
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
