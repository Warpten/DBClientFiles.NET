using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DBClientFiles.NET.IO
{
    internal class LimitedStream : DelegatingStream
    {
        public LimitedStream(Stream underlyingStream, long maxLength, bool disposing = true) : base(underlyingStream, disposing)
        {
            Length = maxLength;
            _remainder = maxLength;
        }

        public override bool CanWrite { get; } = false;

        public override long Length { get; }
        private long _remainder;

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek)
                throw new NotSupportedException();

            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset >= Length)
                        throw new NotSupportedException();
                    break;
                case SeekOrigin.Current:
                    if (Position + offset > Length)
                        throw new NotSupportedException();
                    break;
                case SeekOrigin.End:
                    if (Length < offset)
                        throw new NotSupportedException();
                    break;
            }

            return base.Seek(offset, origin);
        }

        public override int Read(byte[] buffer, int offset, int count)
            => Read(new Span<byte>(buffer, offset, count));

        public override int Read(Span<byte> buffer)
        {
            if (_remainder <= 0)
                return 0;

            var readCount = base.Read(buffer.Slice(0, (int)Math.Min(buffer.Length, _remainder)));
            _remainder -= readCount;
            return readCount;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => base.BeginRead(buffer, offset, (int)Math.Min(count, _remainder), callback, state);

        public override int EndRead(IAsyncResult asyncResult)
        {
            var readCount = base.EndRead(asyncResult);
            _remainder -= readCount;
            return readCount;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => base.ReadAsync(buffer, offset, (int) Math.Min(count, _remainder), cancellationToken);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => base.ReadAsync(buffer.Slice(0, (int) _remainder), cancellationToken);

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override void Write(ReadOnlySpan<byte> buffer)
            => throw new NotSupportedException();

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => throw new NotSupportedException();

        public override void EndWrite(IAsyncResult asyncResult)
            => throw new NotSupportedException();

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
