using DBClientFiles.NET.Internals.Versions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal abstract class SegmentReader<T, TValue> where TValue : class, new()
    {
        private Segment<TValue> _segment;
        protected Segment<TValue> Segment => _segment;
        protected BaseReader<TValue> Reader => _segment.Reader;

        protected SegmentReader(Segment<TValue> segment)
        {
            _segment = segment;
        }

        public abstract IEnumerable<T> Enumerate();
        public abstract void Read();
    }

    internal sealed class OffsetmapReader<TValue> : SegmentReader<(int, long), TValue> where TValue : class, new()
    {
        public OffsetmapReader(Segment<TValue> segment) : base(segment)
        {
        }

        private Dictionary<int, long> _parsedContent = new Dictionary<int, long>();

        public int MinIndex { get; set; }
        public int MaxIndex { get; set; }

        public override IEnumerable<(int, long)> Enumerate()
        {
            if (!Segment.Exists)
                yield break;

            int i = 0;
            Reader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            while (Reader.BaseStream.Position < Segment.EndOffset)
            {
                long offset = Reader.ReadInt32();
                Reader.BaseStream.Seek(2, SeekOrigin.Current);

                ++i;

                if (offset == 0)
                    continue;

                _parsedContent.Add(MinIndex + i - 1, offset);
                yield return (MinIndex + i - 1, offset);
            }
        }

        public override void Read()
        {
            if (!Segment.Exists)
                return;

            int i = 0;
            Reader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            while (Reader.BaseStream.Position < Segment.EndOffset)
            {
                long offset = Reader.ReadInt32();
                Reader.BaseStream.Seek(2, SeekOrigin.Current);

                ++i;

                if (offset == 0)
                    continue;

                _parsedContent.Add(MinIndex + i - 1, offset);
            }
        }

        public long this[int index]
        {
            get => _parsedContent[index];
        }
    }

    internal sealed class StringTableReader<TValue> : SegmentReader<string, TValue> where TValue : class, new()
    {
        public StringTableReader(Segment<TValue> segment) : base(segment)
        {
        }

        private Dictionary<long, string> _stringTable = new Dictionary<long, string>();

        public event Action<long, string> OnStringRead;

        public override IEnumerable<string> Enumerate()
        {
            if (Segment.Length == 0)
                yield break;

            Reader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            while (Reader.BaseStream.Position < Segment.EndOffset)
            {
                var stringPosition = Reader.BaseStream.Position;
                var @string = Reader.ReadStringDirect();

                OnStringRead?.Invoke(stringPosition, @string);

                _stringTable.Add(stringPosition, @string);

                yield return @string;
            }
        }

        public override void Read()
        {
            if (Segment.Length == 0 || OnStringRead == null)
                return;

            Reader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            while (Reader.BaseStream.Position < Segment.EndOffset)
            {
                var stringPosition = Reader.BaseStream.Position;
                var @string = Reader.ReadStringDirect();

                OnStringRead?.Invoke(stringPosition, @string);

                _stringTable.Add(stringPosition, @string);
            }
        }

        public string this[int offset] => _stringTable[offset];
    }
}
