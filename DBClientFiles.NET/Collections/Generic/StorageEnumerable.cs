using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Parsing.File;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using WDBC = DBClientFiles.NET.Parsing.File.WDBC;

namespace DBClientFiles.NET.Collections.Generic
{
    public class StorageEnumerable<T> : IEnumerable<T>
    {
        public StorageOptions Options { get; }
        private IReader<T> _implementation;

        internal int Size { get; private set; }

        public StorageEnumerable(StorageOptions options, Stream dataStream)
        {
            Options = options;
            FromStream(dataStream);
        }

        private void FromStream(Stream dataStream)
        {
#if NETCOREAPP
            Span<byte> identifierBytes = stackalloc byte[4];
            dataStream.Read(identifierBytes);
            var identifier = (Signatures)MemoryMarshal.Read<uint>(identifierBytes);
#else
            var identifierBytes = new byte[4];
            dataStream.Read(identifierBytes, 0, 4);
            var identifier = (Signatures)((identifierBytes[0]) | (identifierBytes[1] << 8) | (identifierBytes[2] << 16) | (identifierBytes[3] << 24));
#endif
            switch (identifier)
            {
                case Signatures.WDBC:
                    _implementation = new WDBC.Reader<T>(Options, dataStream);
                    break;
                default:
                    throw new VersionNotSupportedException(identifier);
            }

            _implementation.Initialize();
            Size = _implementation.Size;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var record in _implementation.Records)
                yield return record.Instance;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
