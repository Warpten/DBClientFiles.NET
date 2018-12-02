using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Parsing.File;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using WDBC = DBClientFiles.NET.Parsing.File.WDBC;
using WDB2 = DBClientFiles.NET.Parsing.File.WDB2;
using WDB5 = DBClientFiles.NET.Parsing.File.WDB5;
using System;

namespace DBClientFiles.NET.Collections.Generic.Internal
{
    internal class Collection<T> : IEnumerable<T>
    {
        private IParser<T> _implementation;
        private Header _header;

        public int RecordCount { get; private set; }

        public ref readonly Header Header => ref _header;
        public ref readonly StorageOptions Options => ref _implementation.Options;

        public Collection(in StorageOptions options, Stream dataStream)
        {
            RecordCount = 0;
            _implementation = null;

            FromStream(in options, dataStream);
        }

        private void FromStream(in StorageOptions options, Stream dataStream)
        {
            Span<byte> identifierBytes = stackalloc byte[4];
            dataStream.Read(identifierBytes);
            var identifier = (Signatures)System.Runtime.InteropServices.MemoryMarshal.Read<uint>(identifierBytes);

            switch (identifier)
            {
                case Signatures.WDBC:
                    _implementation = new WDBC.Parser<T>(in options, dataStream);
                    break;
                case Signatures.WDB2:
                    _implementation = new WDB2.Parser<T>(in options, dataStream);
                    break;
                case Signatures.WDB5:
                    _implementation = new WDB5.Parser<T>(in options, dataStream);
                    break;
                default:
                    throw new VersionNotSupportedException(identifier);
            }

            _implementation.Initialize();

            RecordCount = _implementation.RecordCount;

            _header = new Header(_implementation.Header);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _implementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
