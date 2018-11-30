using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Parsing.File.Segments
{
    internal struct BlockHandlers
    {
        private Dictionary<BlockIdentifier, IBlockHandler> _handlers;

        public bool ReadBlock<T>(T file, Block block) where T : BinaryReader, IParser
        {
            if (!_handlers.TryGetValue(block.Identifier, out var handler))
                return false;

            handler.ReadBlock(file, block.StartOffset, block.Length);
            return true;
        }

        public void Register(IBlockHandler handler)
        {
            if (_handlers == null)
                _handlers = new Dictionary<BlockIdentifier, IBlockHandler>();

            _handlers[handler.Identifier] = handler;
        }

        public void Register<T>() where T : IBlockHandler, new()
        {
            Register(new T());
        }

        public T GetHandler<T>(BlockIdentifier identifier) where T : IBlockHandler
        {
            if (_handlers == null)
                return default;

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
