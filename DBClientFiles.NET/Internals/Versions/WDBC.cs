using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Versions
{
    internal sealed class WDBC<TKey, TValue> : BaseFileReader<TKey, TValue>
        where TValue : class, new()
        where TKey : struct
    {
        public WDBC(IFileHeader header, Stream strm, StorageOptions options) : base(header, strm, options)
        {
        }

        protected override void ReleaseResources()
        {
            StringTable.Dispose();
        }

        public override bool PrepareMemberInformations()
        {
            Debug.Assert(BaseStream.Position == 20);

            Records.StartOffset = BaseStream.Position;
            Records.Length = Header.RecordCount * Header.RecordSize;

            StringTable.StartOffset = Records.EndOffset;
            StringTable.Length = Header.StringTableLength;

            var declaredMemberCount = MemberStore.DeclaredMemberCount();
            if (declaredMemberCount != Header.FieldCount)
                throw new InvalidOperationException($"{typeof(TValue).FullName} declares {declaredMemberCount} members (including arrays), but there should only be {Header.FieldCount}");

            // sets up a default generator
            return base.PrepareMemberInformations();
        }

        protected override IEnumerable<TValue> ReadRecords(int recordIndex, long recordOffset, int recordSize)
        {
            using (var segmentStream = new RecordReader(this, StringTable.Exists, recordSize))
                yield return Generator.Deserialize(this, segmentStream);
        }
        
    }
}
