using DBClientFiles.NET.Parsing.Shared.Headers;
using System.IO;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.Versions.WDBC
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Header : IHeader
    {
        public readonly int RecordCount;
        public readonly int FieldCount;
        public readonly int RecordSize;
        public readonly int StringTableLength;

        public IBinaryStorageFile<T> MakeStorageFile<T>(in StorageOptions options, Stream dataStream)
            => new StorageFile<T>(in options, in this, dataStream);
    }
}
