using System;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers
{
    internal abstract class MultiDictionaryBlockHandler<TKey, TValue> : DictionaryBlockHandler<TKey, List<TValue>>
    {
        protected sealed override void ReadPair(BinaryReader reader)
        {
            var key = ReadKey(reader);
            if (TryGetValue(key, out var value))
                value.Add(ReadValueElement(reader));
            else
                Add(key, new List<TValue>() { ReadValueElement(reader) });
        }

        public abstract TValue ReadValueElement(BinaryReader reader);

        public sealed override List<TValue> ReadValue(BinaryReader reader, TKey key)
        {
            // Should not be called
            throw new InvalidOperationException();
        }
    }
}
