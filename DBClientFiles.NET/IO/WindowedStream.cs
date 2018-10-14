using System;
using System.IO;

namespace DBClientFiles.NET.IO
{
    internal class WindowedStream : Stream
    {
        private Stream _stream;

        private long _windowOffset;
        private int _windowSize;

        public WindowedStream(Stream stream, int windowSize)
        {
            _stream = stream;
            _windowSize = windowSize;
            _windowOffset = stream.Position;
        }

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _windowSize;

        public override long Position {
            get => _stream.Position - _windowOffset;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override void Flush()
            => _stream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position >= _windowSize)
                return 0;

            return _stream.Read(buffer, offset, Math.Min(count - offset, _windowSize));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _stream.Seek(offset - _windowOffset, SeekOrigin.Begin);
                    break;
                case SeekOrigin.Current:
                    _stream.Position += offset;
                    break;
                case SeekOrigin.End:
                    _stream.Seek(_windowOffset + _windowSize + offset, SeekOrigin.Begin);
                    break;
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count > Position + _windowSize)
                throw new ArgumentOutOfRangeException(nameof(count));

            _stream.Write(buffer, offset, count);
        }
    }
}
