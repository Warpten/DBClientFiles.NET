﻿using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Parsing.File;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using WDBC = DBClientFiles.NET.Parsing.File.WDBC;
using WDB2 = DBClientFiles.NET.Parsing.File.WDB2;
using WDB5 = DBClientFiles.NET.Parsing.File.WDB5;

namespace DBClientFiles.NET.Collections.Generic.Internal
{
    internal sealed class Collection<T> : IEnumerable<T>
    {
        private IParser<T> _implementation;
        public int Size { get; private set; }

        public ref readonly IFileHeader Header => ref _implementation.Header;
        public ref readonly StorageOptions Options => ref _implementation.Options;

        public Collection(in StorageOptions options, Stream dataStream)
        {
            FromStream(in options, dataStream);
        }

        private void FromStream(in StorageOptions options, Stream dataStream)
        {
#if NETCOREAPP
            System.Span<byte> identifierBytes = stackalloc byte[4];
            dataStream.Read(identifierBytes);
            var identifier = (Signatures)System.Runtime.InteropServices.MemoryMarshal.Read<uint>(identifierBytes);
#else
            var identifierBytes = new byte[4];
            dataStream.Read(identifierBytes, 0, 4);
            var identifier = (Signatures)((identifierBytes[0]) | (identifierBytes[1] << 8) | (identifierBytes[2] << 16) | (identifierBytes[3] << 24));
#endif

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
            Size = _implementation.Size;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _implementation.Records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}