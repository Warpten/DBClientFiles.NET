using System;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class StringTableSegment<TValue> : SegmentReader<TValue>
        where TValue : class, new()
    {
        public StringTableSegment(FileReader reader) : base(reader) { }

        private byte[] _stringPool;

        public event Action<long, string> OnStringRead;

        public override void Read()
        {
            //if (Segment.Length == 0)
                return;
            
           // Storage.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
           // _stringPool = Storage.ReadBytes((int)Segment.Length);
        }

        protected override void Release()
        {
            _stringPool = null;
        }

        public string this[int offset]
        {
            get
            {
                return null;
            }
        }
    }
}
