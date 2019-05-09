using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Parsing.File;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using WDBC = DBClientFiles.NET.Parsing.File.WDBC;
using WDB2 = DBClientFiles.NET.Parsing.File.WDB2;
using WDB5 = DBClientFiles.NET.Parsing.File.WDB5;
using WDC1 = DBClientFiles.NET.Parsing.File.WDC1;

using System;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Collections.Generic.Internal
{
    /// <summary>
    /// This class is the general entry point for all the implementations of the various generic collections provided by the library.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Collection<T> : IEnumerable<T>
    {
        private IParser<T> _implementation;

        public ref readonly StorageOptions Options => ref _implementation.Options;

        public Collection(in StorageOptions options, Stream dataStream)
        {
            _implementation = null;

            FromStream(in options, dataStream);
        }

        private void FromStream(in StorageOptions options, Stream dataStream)
        {
            Span<byte> identifierBytes = stackalloc byte[4];
            dataStream.Read(identifierBytes);

            var identifier = (Signatures) MemoryMarshal.Read<uint>(identifierBytes);
            dataStream.Seek(-4, SeekOrigin.Current);

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
                case Signatures.WDC1:
                case Signatures.CLS1:
                    _implementation = new WDC1.Parser<T>(in options, dataStream);
                    break;
                default:
                    throw new VersionNotSupportedException(identifier);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _implementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void SkipTo(int recordID)
        {

        }

        public void Reset()
        {

        }
    }
}
