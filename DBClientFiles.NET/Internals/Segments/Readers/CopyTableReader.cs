using DBClientFiles.NET.Utils;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class CopyTableReader<TKey, TValue> : SegmentReader<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        public CopyTableReader()
        {
            _parsedContent = new Dictionary<TKey, TKey>();
        }

        private Dictionary<TKey /* newRow */, TKey /* oldRow */> _parsedContent;

        public int Length => _parsedContent.Count;

        public IEnumerable<TKey> this[TKey oldKey]
        {
            get
            {
                foreach (var kv in _parsedContent)
                    if (kv.Value.Equals(oldKey))
                        yield return kv.Key;
            }
        }

        public override void Read()
        {
            if (!Segment.Exists)
                return;

            Storage.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            while (Storage.BaseStream.Position < Segment.EndOffset)
            {
                var newKey = Storage.ReadStruct<TKey>();
                var oldKey = Storage.ReadStruct<TKey>();
                _parsedContent[newKey] = oldKey;
            }

            Segment.Deserialized = true;
        }

        protected override void Release()
        {
            _parsedContent.Clear();
        }
    }
}
