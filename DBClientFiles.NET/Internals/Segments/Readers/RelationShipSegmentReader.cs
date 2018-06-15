using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.IO;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    //! TOOD Not sure if correct
    internal sealed class RelationShipSegmentReader<TKey> : SegmentReader
        where TKey : struct
    {
        private Dictionary<int /* recordIndex */, byte[]> _entries;

        public RelationShipSegmentReader(FileReader reader) : base(reader)
        {
        }

        public override void Read()
        {
            if (!Segment.Exists)
                return;

            FileReader.BaseStream.Position = Segment.StartOffset;
            var entryCount = FileReader.ReadInt32();
            FileReader.BaseStream.Seek(4 + 4, SeekOrigin.Current);

            _entries = new Dictionary<int, byte[]>();

            for (var i = 0; i < entryCount; ++i)
                _entries[FileReader.ReadInt32()] = FileReader.ReadBytes(4);
        }

        protected override void Release()
        {
            _entries = null;
        }

        public U GetForeignKey<U>(int recordIndex)
            where U : struct
        {
            if (_entries == null || !_entries.ContainsKey(recordIndex))
                return default;
            
            var fk = _entries[recordIndex];
            return MemoryMarshal.Read<U>(fk);
        }
    }
}
