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
    internal sealed class CopyTableReader<TKey> : SegmentReader
        where TKey : struct
    {
        public CopyTableReader(FileReader reader) : base(reader)
        {
            _parsedContent = new Dictionary<TKey, List<TKey>>(reader.Header.RecordCount);
        }

        private readonly Dictionary<TKey /* oldRow */, List<TKey> /* newRows */> _parsedContent;

        public IEnumerable<TKey> this[TKey oldKey] => _parsedContent[oldKey];

        public override void Read()
        {
            if (!Segment.Exists)
                return;

            FileReader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            while (FileReader.BaseStream.Position < Segment.EndOffset)
            {
                var newKey = FileReader.ReadStruct<TKey>();
                var copiedRow = FileReader.ReadStruct<TKey>();

                if (!_parsedContent.TryGetValue(copiedRow, out var block))
                    block = _parsedContent[copiedRow] = new List<TKey>();

                block.Add(newKey);
            }
        }

        protected override void Release()
        {
            _parsedContent.Clear();
        }
    }
}
