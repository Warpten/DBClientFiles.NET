using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.IO
{
    internal class OffsetStream : DelegatingStream
    {
        public long Offset { get; }

        public OffsetStream(Stream underlyingStream, bool disposing = true) : base(underlyingStream, disposing)
        {
            if (!underlyingStream.CanSeek)
                throw new InvalidOperationException("Stream does not support seek operation - offset stream needs an explicit offset value provided to its constructor");

            Offset = underlyingStream.Position;
        }

        public OffsetStream(Stream underlyingStream, long offset, bool disposing = true) : base(underlyingStream, disposing)
        {
            Offset = offset;
        }

        public override long Position {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]

            get => base.Position - Offset;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]

            set => base.Position = value + Offset;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
                offset += Offset;

            return base.Seek(offset, origin);
        }
    }
}
