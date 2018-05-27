using DBClientFiles.NET.Utils;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader for the copy table section of DB2 files.
    /// </summary>
    /// <typeparam name="TKey">The type of key to use.</typeparam>
    /// <typeparam name="TValue">The record type.</typeparam>
    internal sealed class CopyTableReader<TKey, TValue> : SegmentReader<TValue>
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
                var copiedRow = Storage.ReadStruct<TKey>();
                _parsedContent[newKey] = copiedRow;
            }
        }

        protected override void Release()
        {
            _parsedContent.Clear();
        }
    }
}
