using DBClientFiles.NET.Internals.Versions;
using System;
using System.Collections.Generic;
using BinaryReader = DBClientFiles.NET.IO.BinaryReader;

namespace DBClientFiles.NET.Internals.Segments
{
    /// <summary>
    /// A container class for the string table segment of WDxx files.
    /// </summary>
    internal class StringTable : Segment
    {
        private IDictionary<long, string> _stringTable = new Dictionary<long, string>();

        public event Action<long, string> OnStringRead;

        public override void Read(BaseReader reader)
        {
            if (!Exists)
                return;

            reader.BaseStream.Position = StartOffset;
            while (reader.BaseStream.Position < EndOffset)
            {
                var ofs = reader.BaseStream.Position;
                var v = reader.ReadString();

                if (reader.Options.InternStrings)
                    v = string.Intern(v);

                OnStringRead?.Invoke(ofs, v);

                _stringTable.Add(ofs, v);
            }
        }

        public string this[long offset]
        {
            get => _stringTable[offset];
        }

        public override void Dispose()
        {
            _stringTable.Clear();
        }
    }
}
