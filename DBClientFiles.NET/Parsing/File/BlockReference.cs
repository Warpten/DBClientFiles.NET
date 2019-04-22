namespace DBClientFiles.NET.Parsing.File
{
    internal readonly struct BlockReference
    {
        public readonly bool Exists;
        public readonly int Length;
        public readonly int? Offset;

        public BlockReference(bool exists, int length, int? offset)
        {
            Exists = exists;
            Length = length;
            Offset = offset;
        }

        public BlockReference(bool exists, int length) : this(exists, length, null)
        {
        }
    }
}
