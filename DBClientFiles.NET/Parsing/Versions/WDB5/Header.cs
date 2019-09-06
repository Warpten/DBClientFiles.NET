using DBClientFiles.NET.Parsing.Shared.Headers;
using DBClientFiles.NET.Utils;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.Versions.WDB5
{
    internal readonly ref struct IndexOrFlags
    {
        private readonly Union<uint, (ushort Flags, short IndexColumn)> _values;

        public uint FullFlags => _values.Left;
        public short IndexColumn => _values.Right.IndexColumn;
        public ushort PartialFlags => _values.Right.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IndexOrFlags FromLeft(uint left) => new IndexOrFlags(left);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IndexOrFlags(uint left)
        {
            _values = left;
        }
    }

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

        // This could use a better name, but I'm out of ideas
        // The important part is that it's either two u16s or one u32.
        private readonly uint _indexPartialOrFlags;

        /// <summary>
        /// the old header (before 21737) has Build instead of LayoutHash.
        /// Thus the simple solution to check if a file uses the old or the new header
        /// is to check for the high bytes of that field - if any bit is set, it's a layout hash. 
        /// </summary>
        public bool IsLegacyHeader => (_buildOrTableHash & 0xFFFF0000) == 0;

        public int IndexColumn => IsLegacyHeader
            ? 0
            : IndexOrFlags.FromLeft(_indexPartialOrFlags).IndexColumn;

        public uint Flags => IsLegacyHeader
            ? IndexOrFlags.FromLeft(_indexPartialOrFlags).FullFlags
            : IndexOrFlags.FromLeft(_indexPartialOrFlags).PartialFlags;

        public IBinaryStorageFile<T> MakeStorageFile<T>(in StorageOptions options, Stream dataStream)
            => new StorageFile<T>(in options, in this, dataStream);
    }
}
