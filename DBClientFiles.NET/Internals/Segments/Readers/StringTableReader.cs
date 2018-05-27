using System;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class StringTableReader<TValue> : SegmentReader<TValue>
        where TValue : class, new()
    {
        public StringTableReader() : base() { }

        private Dictionary<long, string> _stringTable = new Dictionary<long, string>();

        public event Action<long, string> OnStringRead;

        public override void Read()
        {
            if (Segment.Length == 0)
                return;

            var options = Storage.Options;

            Storage.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            while (Storage.BaseStream.Position < Segment.EndOffset)
            {
                var stringPosition = Storage.BaseStream.Position - Segment.StartOffset;
                var @string = Storage.ReadString();

                OnStringRead?.Invoke(stringPosition, @string);

                _stringTable.Add(stringPosition, Storage.Options.InternStrings ? string.Intern(@string) : @string);
            }
        }

        protected override void Release()
        {
            _stringTable.Clear();
        }

        public string this[int offset] => _stringTable[offset];
    }
}
