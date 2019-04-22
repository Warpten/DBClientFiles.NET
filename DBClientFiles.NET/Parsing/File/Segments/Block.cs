namespace DBClientFiles.NET.Parsing.File.Segments
{
    /// <summary>
    /// A block represents a section of a file. See <see cref="BlockIdentifier"/> for possible semantic meanings.
    /// </summary>
    internal class Block
    {
        public long StartOffset
        {
            get
            {
                if (Previous == null)
                    return 0;

                return Previous.StartOffset + Previous.Length;
            }
        }

        public long Length { get; set; }
        public long EndOffset => StartOffset + Length;

        public bool Exists
        {
            get => Length != 0;
            set
            {
                if (!value)
                    Length = 0;
            }
        }

        public Block Next {
            get => _nextBlock;
            set {
                // Fix the chain
                if (_nextBlock != null)
                {
                    _nextBlock._previousBlock = value;
                    value._nextBlock = _nextBlock;
                }

                // Update our node.
                value._previousBlock = this;
                _nextBlock = value;
            }
        }

        public Block Previous
        {
            get => _previousBlock;
            set {
                if (_previousBlock != null)
                {
                    _previousBlock._nextBlock = value;
                    value._previousBlock = _previousBlock;
                }

                value._nextBlock = this;
                _previousBlock = value;
            }
        }

        public BlockIdentifier Identifier { get; set; }

        private Block _nextBlock = null;
        private Block _previousBlock = null;

        public IBlockHandler Handler { get; set; }

        public bool ReadBlock(IBinaryStorageFile parser)
        {
            if (Handler == null)
                return false;

            Handler.ReadBlock(parser, StartOffset, Length);
            return true;
        }
    }
}
