using DBClientFiles.NET.Internals.Versions;
using System;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal interface ISegmentReader<TValue> : IDisposable
        where TValue : class, new()
    {
        BaseFileReader<TValue> Storage { get; }
        Segment<TValue> Segment { get; set; }
        void Read();
    }

    internal abstract class SegmentReader<TValue> : ISegmentReader<TValue> where TValue : class, new()
    {
        public Segment<TValue> Segment { get; set; }

        public BaseFileReader<TValue> Storage => Segment.Storage;

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
