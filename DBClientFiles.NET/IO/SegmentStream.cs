using System;
using System.IO;

namespace DBClientFiles.NET.IO
{
    internal class SegmentStream : Stream
    {
        private Stream _underlyingStream;
        private long _length;

        private long StartOffset { get; }

        private long EndOffset => StartOffset + _length;

        public SegmentStream(Stream underlyingStream, long offset, long length)
        {
            _underlyingStream = underlyingStream;
            StartOffset = offset;
            _length = length;
        }

        public override bool CanRead => _underlyingStream.CanRead;
        public override bool CanSeek => _underlyingStream.CanSeek;
        public override bool CanWrite => _underlyingStream.CanWrite;

        public override long Length => _length;

        public override long Position {
            get => _underlyingStream.Position - StartOffset;
            set {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override void Flush() => _underlyingStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            _underlyingStream.Seek(StartOffset, SeekOrigin.Begin);

            count = (int)Math.Min(Length - Position, count);
            return _underlyingStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Current:
                    if (_underlyingStream.Position + offset > EndOffset)
                        throw new InvalidOperationException("Seeking out of stream!");
                    return _underlyingStream.Seek(offset, origin);
                case SeekOrigin.Begin:
                {
                    // Offset relative to the underlying stream's beginning
                    var underlyingRelativeOffset = StartOffset + offset;
                    if (underlyingRelativeOffset > EndOffset)
                        throw new InvalidProgramException("Seeking out of stream!");
                    return _underlyingStream.Seek(underlyingRelativeOffset, origin);
                }
                case SeekOrigin.End:
                {
                    // Calculate distance of our end from substreams's
                    var underlyingRelativeOffset = _underlyingStream.Length - EndOffset;
                    offset -= underlyingRelativeOffset; // Offset is usually negative here
                    return _underlyingStream.Seek(offset, origin);
                }
            }

            // Unreachable
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            count = (int)Math.Min(Length - Position, count);
            _underlyingStream.Write(buffer, offset, count);
        }
    }
}
