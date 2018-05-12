using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Internals.Versions
{
    /*internal class WDB6<TKey, TValue> : BaseReader where TKey : struct
    {
        public WDB6(Stream strm) : base(strm, true)
        {

        }


        public override IEnumerable<TValue> ReadRecords()
        {
            var cache = new LegacySerializer<TKey, TValue>(this);

            var copyTable = new Dictionary<TKey, List<TKey>>();
            if (CopyTable.Exists)
            {
                BaseStream.Position = CopyTable.StartOffset;
                foreach (var pair in CopyTable.Read<TKey>(this))
                {
                    if (!copyTable.TryGetValue(pair.Key, out var node))
                        node = copyTable[pair.Key] = new List<TKey>();

                    node.Add(pair.Value);
                }
            }

            var i = 0;
            BaseStream.Position = Records.StartOffset;
            while (BaseStream.Position < Records.EndOffset)
            {
                var oldStructure = cache.Deserialize(this);

                BaseStream.Position = OffsetMap[i++];
                var currentKey = cache.ExtractKey(oldStructure);

                if (copyTable.TryGetValue(currentKey, out var nodes))
                {
                    for (var itr = 0; itr < nodes.Count; ++itr)
                    {
                        var clone = cache.Clone(oldStructure);
                        cache.InsertKey(clone, nodes[itr]);
                        yield return clone;
                    }
                }

                yield return oldStructure;
            }
        }
    }

    internal abstract class WDB6 : BaseReader
    {
        protected WDB6(Stream strm) : base(strm, true)
        {

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override bool ReadHeader()
        {
            var recordCount = ReadInt32();
            if (recordCount == 0)
                return false;

            FieldCount = ReadInt32();
            var recordSize = ReadInt32();
            StringTable.Length = ReadInt32();

            // Table hash; Layout hash
            BaseStream.Position += 4 + 4;

            var minIndex = ReadInt32();
            var maxIndex = ReadInt32();

            BaseStream.Position += 4; // Locale

            var copyTableSize = ReadInt32();
            var flags = ReadInt16();
            var indexColumn = ReadInt32();
            var totalFieldCount = ReadInt32();
            CommonTable = new CommonTable
            {
                Length = ReadInt32()
            };

            StringTable.Exists = (flags & 0x01) == 0;

            OffsetMap.Exists = (flags & 0x01) != 0;
            OffsetMap.Length = (maxIndex - minIndex + 1) * (4 + 2);
            if (OffsetMap.Exists) // not a typo - if flag is set old length is an absolute offset
                OffsetMap.StartOffset = StringTable.Length;

            IndexTable.Exists = (flags & 0x04) != 0;
            IndexTable.StartOffset = OffsetMap.EndOffset;
            IndexTable.Length = recordCount * 4;

            CopyTable.StartOffset = IndexTable.EndOffset;
            CopyTable.Length = copyTableSize;
            CopyTable.Exists = CopyTable.Length != 0;

            CommonTable.StartOffset = CopyTable.EndOffset;

            // TODO: Check that the mapped index column corresponds to metadata

            return true;
        }
    }*/
}
