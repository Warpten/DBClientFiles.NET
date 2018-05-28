using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class RelationShipSegmentReader<TKey, T> : SegmentReader<T> where T : class, new()
    {
        private Dictionary<int /* recordIndex */, byte[]> _entries;

        public override void Read()
        {
            if (!Segment.Exists)
                return;

            Storage.BaseStream.Position = Segment.StartOffset;
            var entryCount = Storage.ReadInt32();
            var minIndex = Storage.ReadStruct<TKey>();
            var maxIndex = Storage.ReadStruct<TKey>();

            _entries = new Dictionary<int, byte[]>();

            for (var i = 0; i < entryCount; ++i)
            {
                _entries[Storage.ReadInt32()] = Storage.ReadBytes(4);
            }
        }

        protected override void Release()
        {
            _entries = null;
        }

        public unsafe U GetForeignKey<U>(int recordIndex) where U : struct
        {
            if (!_entries.ContainsKey(recordIndex))
                return default;

            //! TODO: prevent long, this will cook us.
            var fk = _entries[recordIndex];
            fixed (byte* b = fk)
                return FastStructure<U>.PtrToStructure(new IntPtr(b));
        }
    }
}
