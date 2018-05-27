using DBClientFiles.NET.Utils;
using System;
using System.Linq;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class RelationShipSegmentReader<TKey, T> : SegmentReader<T> where T : class, new()
    {
        private class RelationshipNode
        {
            public int RecordIndex { get; set; }
            public byte[] ForeignKey { get; set; }
        }

        private RelationshipNode[] _entries;

        public override void Read()
        {
            if (!Segment.Exists)
                return;

            Storage.BaseStream.Position = Segment.StartOffset;
            var entryCount = Storage.ReadInt32();
            var minIndex = Storage.ReadStruct<TKey>();
            var maxIndex = Storage.ReadStruct<TKey>();

            _entries = new RelationshipNode[entryCount];

            for (var i = 0; i < entryCount; ++i)
            {
                _entries[i] = new RelationshipNode();
                _entries[i].RecordIndex = Storage.ReadInt32();
                _entries[i].ForeignKey = Storage.ReadBytes(4);
            }
        }

        protected override void Release()
        {
            _entries = null;
        }

        public unsafe U GetForeignKey<U>(int recordIndex) where U : struct
        {
            //! TODO: prevent long, this will cook us.
            var fk = _entries.First(e => e.RecordIndex == recordIndex).ForeignKey;
            fixed (byte* b = fk)
                return FastStructure<U>.PtrToStructure(new IntPtr(b));
        }
    }
}
