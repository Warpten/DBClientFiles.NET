using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Parsing.Shared.Headers;
using DBClientFiles.NET.Parsing.Versions;
using DBClientFiles.NET.Utils.Extensions;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing
{
    using WDB2 = Versions.WDB2;
    using WDB5 = Versions.WDB5;
    using WDBC = Versions.WDBC;
    using WDC1 = Versions.WDC1;

    internal static class BinaryStorageFactory<T>
    {
        public static unsafe IBinaryStorageFile<T> Process(in StorageOptions options, Stream dataStream)
        {
            dataStream = dataStream.MakeSeekable();

            Signatures signature = default;
            Span<byte> bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref signature, 1));
            if (dataStream.Read(bytes) != 4)
                throw new InvalidOperationException("Unable to read dbc/db2 signature: stream too small");

            switch (signature)
            {
                case Signatures.WDBC:
                    return Process<WDBC.Header>(in options, dataStream);
                case Signatures.WDB2:
                    return Process<WDB2.Header>(in options, dataStream);
                case Signatures.WDB5:
                    return Process<WDB5.Header>(in options, dataStream);
                case Signatures.WDC1:
                    return Process<WDC1.Header>(in options, dataStream);
                default:
                    throw new VersionNotSupportedException($"Unhandled file signature {signature} ({(uint) signature:X8})");
            }
        }

        private static unsafe IBinaryStorageFile<T> Process<THeader>(in StorageOptions options, Stream dataStream) where THeader : struct, IHeader
        {
            THeader header = default;
            var headerBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref header, 1));

            var readCount = dataStream.Read(headerBytes);
            if (readCount != Unsafe.SizeOf<THeader>())
                throw new InvalidOperationException($"Unable to read header from stream: {Unsafe.SizeOf<THeader>()} bytes expected, got only {readCount}.");

            // Encapsulate a new stream in a wrapper where header offset is considered.
            var windowedStream = dataStream.Rebase(true);
            return header.MakeStorageFile<T>(in options, windowedStream);
        }
    }
}
