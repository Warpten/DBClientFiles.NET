using DBClientFiles.NET.Parsing.Versions;
using DBClientFiles.NET.Utils.Extensions;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace DBClientFiles.NET.Parsing
{
    using WDBC = Versions.WDBC;
    using WDB2 = Versions.WDB2;
    using WDB5 = Versions.WDB5;

    internal static class FileHeaderParser<T>
    {
        public static IBinaryStorageFile<T> Process(Stream dataStream)
        {
            Span<byte> bytes = stackalloc byte[4];
            if (dataStream.Read(bytes) != 4)
                throw new InvalidOperationException("Unable to read dbc/db2 signature: stream too small");

            var magicIdentifier = (Signatures) MemoryMarshal.Read<uint>(bytes);
            switch (magicIdentifier)
            {
                case Signatures.WDBC:
                    return Process<WDBC.Segments.Handlers.Header>(dataStream);
                case Signatures.WDB2:
                    return Process<WDB2.Segments.Handlers.Header>(dataStream);
                case Signatures.WDB5:
                    return Process<WDB5.Segments.Handlers.Header>(dataStream);
                default:
                    throw new InvalidOperationException($"Unhandled file signature {magicIdentifier} ({(uint) magicIdentifier:X8})");
            }
        }

        private static unsafe IBinaryStorageFile<T> Process<THeader>(Stream dataStream) where THeader : struct
        {
            THeader header = default;

            var headerBytes = new Span<byte>(Unsafe.AsPointer(ref header), Unsafe.SizeOf<THeader>());

            var readCount = dataStream.Read(headerBytes);
            if (readCount != Unsafe.SizeOf<THeader>())
                throw new InvalidOperationException($"Unable to read header from stream: {Unsafe.SizeOf<THeader>()} bytes expected, got only {readCount}.");

            // Encapsulate a new stream in a wrapper where header offset is considered.
            var windowedStream = dataStream.Rebase(Unsafe.SizeOf<THeader>() + 4, true);

            return null;
        }
    }
}
