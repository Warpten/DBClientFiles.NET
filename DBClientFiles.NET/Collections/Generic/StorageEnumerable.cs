using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Parsing.File;
using DBClientFiles.NET.Parsing.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using WDBC = DBClientFiles.NET.Parsing.File.WDBC;

namespace DBClientFiles.NET.Collections
{
    internal sealed class StorageEnumerable<T> : IEnumerable<T>
    {
        private IReader<T> _implementation;
        public int Size { get; private set; }

        public IFileHeader Header => _implementation.Header;
        public StorageOptions Options => _implementation.Options;
        public ISerializer<T> Serializer => _implementation.Serializer;

        public StorageEnumerable(StorageOptions options, Stream dataStream)
        {
            FromStream(options, dataStream);
        }

        private void FromStream(StorageOptions options, Stream dataStream)
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
                    _implementation = new WDBC.Reader<T>(options, dataStream);
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
            => GetEnumerator();
    }

}
