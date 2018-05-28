using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments.Readers;
using DBClientFiles.NET.Internals.Versions;
using System;

namespace DBClientFiles.NET.Internals.Segments
{
    internal class Segment<TValue, TReader> : Segment<TValue>
        where TValue : class, new()
        where TReader : class, ISegmentReader<TValue>, new()
    {
        public TReader Reader { get; private set; }

        internal Segment(BaseFileReader<TValue> storage) : base(storage)
        {
            Reader = new TReader
            {
                Segment = this
            };
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Reader.Dispose();
        }
    }

    internal class Segment<TValue> : IEquatable<Segment<TValue>>, IDisposable
        where TValue : class, new()
    {
        public long StartOffset { get; set; }
        public long Length { get; set; }

        public long EndOffset => Exists ? (StartOffset + Length) : StartOffset;
        
        public BaseFileReader<TValue> Storage { get; private set; }
        private StorageOptions Options { get; set; }

        public bool Exists
        {
            set
            {
                if (!value)
                    Length = 0;
            }
            get => Length != 0;
        }

        internal Segment(BaseFileReader<TValue> storage)
        {
            Options = storage.Options;
            Storage = storage;

            StartOffset = 0;
            Length = 0;
            
        }

        internal Segment()
        {
            StartOffset = 0;
            Length = 0;
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Options = null;
                    Storage = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public bool Equals(Segment<TValue> other)
        {
            if (other == null)
                return false;

            return StartOffset == other.StartOffset && Length == other.Length;
        }
    }
}
