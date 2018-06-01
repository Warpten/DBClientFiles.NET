using DBClientFiles.NET.IO;
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
    internal sealed class CopyTableReader<TKey> : SegmentReader
        where TKey : struct
    {
        public CopyTableReader(FileReader reader) : base(reader)
        {
            _parsedContent = new Dictionary<TKey, TKey>();
        }

        private readonly Dictionary<TKey /* newRow */, TKey /* oldRow */> _parsedContent;

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

            FileReader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            while (FileReader.BaseStream.Position < Segment.EndOffset)
            {
                var newKey = FileReader.ReadStruct<TKey>();
                var copiedRow = FileReader.ReadStruct<TKey>();
                _parsedContent[newKey] = copiedRow;
            }
        }

        protected override void Release()
        {
            _parsedContent.Clear();
        }
    }
}
