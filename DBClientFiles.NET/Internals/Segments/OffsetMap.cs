using DBClientFiles.NET.Internals.Versions;
using System.Collections.Generic;

namespace DBClientFiles.NET.Internals.Segments
{
    internal class OffsetMap : Segment
    {
        private long[] _values;

        public override void Read(BaseReader reader)
        {
            if (!Exists)
                return;

            _values = new long[Length / (4 + 2)];

            reader.BaseStream.Position = StartOffset;
            var i = 0;
            while (reader.BaseStream.Position < EndOffset)
            {
                // Values at offset 0 should be ignored, and we skip lenght of record

                var value = reader.ReadInt32();
                while (value == 0)
                {
                    reader.BaseStream.Position += 2;
                    value = reader.ReadInt32();
                }
                _values[i++] = value;
                reader.BaseStream.Position += 2;
            }
        }

        public long this[int index] => _values[index];

        public override void Dispose()
        {
            base.Dispose();

            _values = null;
        }
    }
}
