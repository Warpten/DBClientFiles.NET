using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DBClientFiles.NET.IO
{
    internal class DelegatingStream : Stream
    {
        private readonly Stream _implementation;
        private readonly bool _disposing;

        public DelegatingStream(Stream underlyingStream, bool disposing = true)
        {
            _implementation = underlyingStream;
            _disposing = disposing;
        }

        public override bool CanRead => _implementation.CanRead;
        public override bool CanSeek => _implementation.CanSeek;
        public override bool CanWrite => _implementation.CanWrite;
        public override bool CanTimeout => _implementation.CanTimeout;

        public override long Position {
            get => _implementation.Position;
            set => _implementation.Position = value;
        }

        public override long Length => _implementation.Length;
        public override int ReadTimeout {
            get => _implementation.ReadTimeout;
            set => _implementation.ReadTimeout = value;
        }

        public override int WriteTimeout {
            get => _implementation.WriteTimeout;
            set => _implementation.WriteTimeout = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetLength(long value) => _implementation.SetLength(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => _implementation.BeginRead(buffer, offset, count, callback, state);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => _implementation.BeginWrite(buffer, offset, count, callback, state);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Close()
            => _implementation.Close();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CopyTo(Stream destination, int bufferSize)
            => _implementation.CopyTo(destination, bufferSize);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => _implementation.CopyToAsync(destination, bufferSize, cancellationToken);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Read(byte[] buffer, int offset, int count)
            => _implementation.Read(buffer, offset, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Read(Span<byte> buffer)
            =>  _implementation.Read(buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => _implementation.ReadAsync(buffer, offset, count, cancellationToken);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => _implementation.ReadAsync(buffer, cancellationToken);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadByte()
            => _implementation.ReadByte();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long Seek(long offset, SeekOrigin origin)
            => _implementation.Seek(offset, origin);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(byte[] buffer, int offset, int count)
            => _implementation.Write(buffer, offset, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(ReadOnlySpan<byte> buffer)
            => _implementation.Write(buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => _implementation.WriteAsync(buffer, offset, count, cancellationToken);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            => _implementation.WriteAsync(buffer, cancellationToken);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteByte(byte value)
            => _implementation.WriteByte(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void EndWrite(IAsyncResult asyncResult)
            => _implementation.EndWrite(asyncResult);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Flush()
            => _implementation.Flush();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int EndRead(IAsyncResult asyncResult)
            => _implementation.EndRead(asyncResult);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task FlushAsync(CancellationToken cancellationToken)
            => _implementation.FlushAsync(cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (_disposing)
                _implementation.Dispose();

            base.Dispose(disposing);
        }
    }
}
