using DBClientFiles.NET.Internals.Versions;
using System;
using System.Collections.Generic;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal interface ISegmentReader<TValue> : IDisposable
        where TValue : class, new()
    {
        Segment<TValue> Segment { get; set; }
        void Read();
    }

    internal abstract class SegmentReader<T, TValue> : ISegmentReader<TValue> where TValue : class, new()
    {
        private Segment<TValue> _segment;
        public Segment<TValue> Segment
        {
            get => _segment;
            set => _segment = value;
        }

        internal BaseReader<TValue> Storage => _segment.Storage;

        protected SegmentReader()
        {

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
