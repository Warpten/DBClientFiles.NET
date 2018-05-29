using DBClientFiles.NET.IO;
using System;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal interface ISegmentReader<TValue> : IDisposable
        where TValue : class, new()
    {
        Segment<TValue> Segment { get; }
        bool Exists { get; }
    }

    internal abstract class SegmentReader<TValue> : ISegmentReader<TValue> where TValue : class, new()
    {
        private Segment<TValue> _segment;
        public Segment<TValue> Segment => _segment;
        public FileReader FileReader { get; }

        public bool Exists
        {
            get => _segment.Exists;
            set => _segment.Exists = value;
        }

        public long StartOffset
        {
            get => _segment.StartOffset;
            set => _segment.StartOffset = value;
        }

        public long Length
        {
            get => _segment.Length;
            set => _segment.Length = value;
        }

        public long EndOffset => _segment.EndOffset;

        protected SegmentReader(FileReader fileReader)
        {
            FileReader = fileReader;
        }

        public abstract void Read();
        protected abstract void Release();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    Release();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
