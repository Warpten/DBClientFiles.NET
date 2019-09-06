using System;
using System.IO;

namespace DBClientFiles.NET.IO
{
    internal class LimitedStream : DelegatingStream
    {
        public LimitedStream(Stream underlyingStream, long maxLength, bool disposing = true) : base(underlyingStream, disposing)
        {
            Length = maxLength;
        }

        public override long Length { get; }

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

            return base.Seek(offset, SeekOrigin.Begin);
        }
    }
}
