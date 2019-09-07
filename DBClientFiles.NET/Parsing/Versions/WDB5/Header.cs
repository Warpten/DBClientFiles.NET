using DBClientFiles.NET.Parsing.Shared.Headers;
using DBClientFiles.NET.Utils;
using System.IO;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.Versions.WDB5
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Header : IHeader
    {
        public readonly int RecordCount;
        public readonly int FieldCount;
        public readonly int RecordSize;
        public readonly int StringTableLength;
        private readonly uint _buildOrTableHash;
        public readonly uint LayoutHash;
        public readonly int MinIndex;
        public readonly int MaxIndex;
        public readonly int Locale;
        public readonly int CopyTableLength;

        /// <summary>
        /// This could use a better name, but I'm out of ideas.
        /// <br/>
        /// The important part is that it's either <c>(ushort Flags, short IndexColumn)</c> or <c>uint Flags</c>.
        /// </summary>
        private readonly Variant<uint> _variant1;

        /// <summary>
        /// WDB5 files before build 21737 were using <c>uint Build</c> instead of <c>uint LayoutHash</c>.
        /// <br/>
        /// Thus, a simple solution to check whether a file uses the old or the new header
        /// is to check for the high bytes of that field - if any bit is set, it's a layout hash.
        /// This just works <sup>(tm)</sup> because build numbers were never past 65536.
        /// </summary>
        public bool IsLegacyHeader => (_buildOrTableHash & 0xFFFF0000) == 0;

        public int IndexColumn => IsLegacyHeader
            ? 0 // Assume first column
            : _variant1.Cast<(ushort Flags, short IndexColumn)>().IndexColumn;

        public uint Flags => IsLegacyHeader
            ? _variant1.Value
            : _variant1.Cast<(ushort Flags, short IndexColumn)>().Flags;

        public IBinaryStorageFile<T> MakeStorageFile<T>(in StorageOptions options, Stream dataStream)
            => new StorageFile<T>(in options, in this, dataStream);
    }
}
