using System;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Parsing.Shared.Segments.Handlers
{
    internal abstract class MultiDictionaryBlockHandler<TKey, TValue> : DictionaryBlockHandler<TKey, List<TValue>>
        where TKey : notnull
    {
        protected sealed override void ReadPair(Stream dataStream)
        {
            var key = ReadKey(dataStream);
            if (TryGetValue(key, out var value))
                value.Add(ReadValueElement(dataStream));
            else
                Add(key, new List<TValue>() { ReadValueElement(dataStream) });
        }

        protected abstract TValue ReadValueElement(Stream dataStream);

        protected sealed override List<TValue> ReadValue(Stream dataStream, TKey key)
        {
            // Should not be called
            throw new InvalidOperationException();
        }
    }
}
