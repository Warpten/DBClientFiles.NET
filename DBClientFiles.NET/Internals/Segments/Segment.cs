namespace DBClientFiles.NET.Internals.Segments
{
    internal struct Segment
    {
        public long StartOffset { get; set; }
        public int Length { get; set; }

        public long EndOffset => StartOffset + Length;

        public bool Exists {
            set {
                if (!value)
                    Length = 0;
            }
            get => Length != 0;
        }
    }
}
