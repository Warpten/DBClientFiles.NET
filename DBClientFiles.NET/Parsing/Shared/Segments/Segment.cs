using DBClientFiles.NET.Parsing.Versions;

namespace DBClientFiles.NET.Parsing.Shared.Segments
{

    /// <summary>
    /// A block represents a section of a file. See <see cref="SegmentIdentifier"/> for possible semantic meanings.
    /// </summary>
    internal class Segment
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
        public Segment Next {
            get => _nextSegment;
            set {
                // Fix the chain
                if (_nextSegment != null)
                {
                    _nextSegment._previousSegment = value;
                    value._nextSegment = _nextSegment;
                }

                // Update our node.
                value._previousSegment = this;
                _nextSegment = value;
            }
        }

        public Segment Previous
        {
            get => _previousSegment;
            set {
                if (_previousSegment != null)
                {
                    _previousSegment._nextSegment = value;
                    value._previousSegment = _previousSegment;
                }

                value._nextSegment = this;
                _previousSegment = value;
            }
        }

        public SegmentIdentifier Identifier { get; set; }

        public ISegmentHandler Handler { get; set; }

        private Segment _nextSegment = null;
        private Segment _previousSegment = null;

        public bool ReadSegment(IBinaryStorageFile parser)
        {
            if (Handler == null)
                return false;

            Handler.ReadSegment(parser, StartOffset, Length);
            return true;
        }
    }
}
