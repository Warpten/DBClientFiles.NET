using System;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Serializers;

namespace DBClientFiles.NET.Internals.Versions
{
    internal class WDB2<TValue> : BaseReader<TValue> where TValue : class, new()
    {
        private long _stringBlockOffset;
        private int _stringBlockSize;
        private int _recordSize;
        private int _recordCount;
        private long _dataOffset;

        public WDB2(Stream fileStream) : base(fileStream, true)
        {
        }

        public override bool ReadHeader()
        {
            _recordCount = ReadInt32();
            if (_recordCount == 0)
                return false;

            FieldCount = ReadInt32();
            _recordSize = ReadInt32();
            _stringBlockSize = ReadInt32();
            var tableHash = ReadInt32();
            var build = ReadInt32();
            var lastWrittenTime = ReadInt32();
            var minId = ReadInt32();
            var maxId = ReadInt32();
            var locale = ReadInt32();
            var copyTableSize = ReadInt32();

            // Skip string length information (unused by nearly everyone)
            if (maxId != 0)
                BaseStream.Position += (maxId - minId + 1) * (4 + 2);

            _dataOffset = BaseStream.Position;

            _stringBlockOffset = BaseStream.Position + _recordSize * _recordCount;

            return true;
        }

        public override IEnumerable<TValue> ReadRecords()
        {
            var cache = new LegacySerializer<TValue>(this);

            BaseStream.Position = _dataOffset;
            for (var i = 0; i < _recordCount; ++i)
                yield return cache.Deserialize();
        }
    }
}
