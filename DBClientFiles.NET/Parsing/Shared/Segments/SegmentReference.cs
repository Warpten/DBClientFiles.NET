namespace DBClientFiles.NET.Parsing.Shared.Segments
{
    internal readonly struct SegmentReference
    {
        public readonly int Length;
        public readonly int? Offset;
        public readonly bool Exists;

        public SegmentReference(bool exists, int length, int? offset)
        {
            Exists = exists;
            Length = length;
            Offset = offset;
        }

        public SegmentReference(bool exists, int length) : this(exists, length, null)
        {
        }


        private static readonly SegmentReference _missing = new SegmentReference(false, 0);
        public static ref readonly SegmentReference Missing => ref _missing;
    }
}
