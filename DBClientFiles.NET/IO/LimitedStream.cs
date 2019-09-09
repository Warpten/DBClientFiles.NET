using System;
using System.IO;
using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset >= Length)
                        throw new NotSupportedException();
                    _remainder = Length - offset;
                    break;
                case SeekOrigin.Current:
                    if (offset > _remainder)
                        throw new NotSupportedException();
                    _remainder -= offset;
                    break;
                case SeekOrigin.End:
                    if (Length < -offset)
                        throw new NotSupportedException();
                    _remainder = -offset;
                    break;
            }

            return base.Seek(offset, origin);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Read(byte[] buffer, int offset, int count)
            => Read(new Span<byte>(buffer, offset, count));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Read(Span<byte> buffer)
        {
            var readCount = base.Read(buffer.Slice(0, Math.Min(buffer.Length, (int) _remainder)));
            _remainder -= readCount;
            return readCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => base.BeginRead(buffer, offset, Math.Min(count, (int) _remainder), callback, state);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int EndRead(IAsyncResult asyncResult)
        {
            var readCount = base.EndRead(asyncResult);
            _remainder -= readCount;
            return readCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => base.ReadAsync(buffer, offset, Math.Min(count, (int) _remainder), cancellationToken);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => base.ReadAsync(buffer.Slice(0, (int) _remainder), cancellationToken);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadByte()
        {
            --_remainder;
            return base.ReadByte();
        }

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
