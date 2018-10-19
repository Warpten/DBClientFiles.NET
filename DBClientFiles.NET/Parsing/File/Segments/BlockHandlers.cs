using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments
{
    internal class BlockHandlers
    {
        private Dictionary<BlockIdentifier, IBlockHandler> _handlers = new Dictionary<BlockIdentifier, IBlockHandler>();

        public void ReadBlock<T, U>(T file, Block block) where T : BinaryReader, IReader<U>
        {
            if (!_handlers.TryGetValue(block.Identifier, out var handler))
                return;

            handler.ReadBlock<T, U>(file, block.StartOffset, block.Length);
        }

        public void Register(IBlockHandler handler)
        {
            _handlers[handler.Identifier] = handler;
        }

        public void Register<T>() where T : IBlockHandler, new()
        {
            Register(new T());
        }

        public T GetHandler<T>(BlockIdentifier identifier) where T : IBlockHandler
        {
            if (_handlers.TryGetValue(identifier, out var handler))
                return (T) handler;

            return default;
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
