namespace DBClientFiles.NET.Parsing.File
{
    internal readonly struct BlockReference
    {
        public readonly int Length;
        public readonly int? Offset;
        public readonly bool Exists;

        public BlockReference(bool exists, int length, int? offset)
        {
            Exists = exists;
            Length = length;
            Offset = offset;
        }

        public BlockReference(bool exists, int length) : this(exists, length, null)
        {
        }

        private static BlockReference _missing = new BlockReference(false, 0);
        public static ref readonly BlockReference Missing => ref _missing;
    }
}
