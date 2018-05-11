using System;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
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
