using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal sealed class CopyTableHandler<TKey> : IBlockHandler
    {
        private Dictionary<TKey, List<TKey>> _store = new Dictionary<TKey, List<TKey>>();

        public BlockIdentifier Identifier { get; } = BlockIdentifier.CopyTable;

        public void Parse<T, U>(T reader, long startOffset, long length) where T : BinaryReader, IReader<U>
        {
            if (startOffset == 0 || length == 0)
                return;

            reader.BaseStream.Seek(startOffset, SeekOrigin.Begin);
            while (reader.BaseStream.Position <= (startOffset + length))
            {
                // Read stuff.
            }
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
