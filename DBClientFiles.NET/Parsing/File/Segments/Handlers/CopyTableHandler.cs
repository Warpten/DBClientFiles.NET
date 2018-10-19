using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal sealed class CopyTableHandler<TKey> : IBlockHandler
    {
        private Dictionary<TKey, List<TKey>> _store = new Dictionary<TKey, List<TKey>>();

        public BlockIdentifier Identifier { get; } = BlockIdentifier.CopyTable;

        public void ReadBlock<T, U>(T reader, long startOffset, long length) where T : BinaryReader, IReader<U>
        {
            if (startOffset == 0 || length == 0)
                return;

            reader.BaseStream.Seek(startOffset, SeekOrigin.Begin);
            while (reader.BaseStream.Position <= (startOffset + length))
            {
                // Read stuff.
            }
        }

        public void WriteBlock<T, U>(T writer) where T : BinaryWriter, IWriter<U>
        {
        }

        public IEnumerable<TKey> this[TKey index]
        {
            get
            {
                if (_store.TryGetValue(index, out var collection))
                    return collection;

                return default;
            }
        }
    }
}
