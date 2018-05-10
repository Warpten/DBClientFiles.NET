using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDB5<TKey, TValue> : WDB5<TValue> where TKey : struct where TValue : class, new()
    {
        public WDB5(Stream strm) : base(strm)
        {

        }


        public override IEnumerable<TValue> ReadRecords()
        {
            var cache = new LegacySerializer<TKey, TValue>(this);

            var copyTable = new Dictionary<TKey, List<TKey>>();
            if (CopyTable.Exists)
            {
                BaseStream.Position = CopyTable.StartOffset;
                foreach (var pair in CopyTable.Enumerate<TKey, TValue>(this))
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

    internal abstract class WDB5<TValue> : BaseReader<TValue> where TValue : class, new()
    {
        protected WDB5(Stream strm) : base(strm, true)
        {

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
            var indexColumn = ReadInt16();

            StringTable.Exists = (flags & 0x01) == 0;
            OffsetMap.Exists = (flags & 0x01) != 0;
            OffsetMap.Length = (maxIndex - minIndex + 1) * (4 + 2);
            if (OffsetMap.Exists)
                OffsetMap.StartOffset = StringTable.Length;

            // TODO: Check that the mapped index column corresponds to metadata

            return true;
        }
    }
}
