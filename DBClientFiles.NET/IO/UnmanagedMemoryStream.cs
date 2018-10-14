using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.IO
{
    internal unsafe class UnmanagedMemoryStream : Stream
    {
        private IntPtr _data;
        private long _cursor = 0;

        public UnmanagedMemoryStream(IntPtr data, long size)
        {
            _data = data;
            Length = size;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;

        public override long Length { get; }
        public override long Position {
            get => _cursor;
            set => _cursor = value;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readCount = 0;
            for (; offset < count && _cursor < Length; ++offset)
            {
                buffer[offset] = ((byte*)_data)[_cursor + readCount];
                ++readCount;
            }
            return readCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var data = (byte*)_data;

            for (; offset < count && _cursor < Length; ++offset)
            {
                data[_cursor] = buffer[offset];

                ++_cursor;
            }
        }
    }
}
