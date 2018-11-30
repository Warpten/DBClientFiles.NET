using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal sealed class CopyTableHandler : IBlockHandler
    {
        private Dictionary<int, List<int>> _store = new Dictionary<int, List<int>>();

        public BlockIdentifier Identifier { get; } = BlockIdentifier.CopyTable;

        public void ReadBlock<T>(T reader, long startOffset, long length) where T : BinaryReader, IParser
        {
            if (startOffset == 0 || length == 0)
                return;

            reader.BaseStream.Seek(startOffset, SeekOrigin.Begin);
            while (reader.BaseStream.Position <= (startOffset + length))
            {
                var key = reader.ReadInt32();
                var value = reader.ReadInt32();

                if (!_store.TryGetValue(key, out var list))
                    list = _store[key] = new List<int>();

                list.Add(value);
            }
        }

        public void WriteBlock<T, U>(T writer) where T : BinaryWriter, IWriter<U>
        {
        }

        public IReadOnlyList<int> this[int index]
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
