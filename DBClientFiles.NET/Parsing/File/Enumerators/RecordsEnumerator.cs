using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DBClientFiles.NET.Parsing.File.Enumerators
{
    class RecordsEnumerator<TValue, TSerializer> : Enumerator<TValue, TSerializer>
        where TSerializer : ISerializer<TValue>, new()
    {
        private Block _block;

        public RecordsEnumerator(BinaryFileParser<TValue, TSerializer> impl) : base(impl)
        {
            _block = FileParser.FindBlock(BlockIdentifier.Records);
            Debug.Assert(_block != null, "Records block missing in enumerator");

            FileParser.BaseStream.Position = _block.StartOffset;
        }

        internal override TValue ObtainCurrent()
        {
            if (FileParser.BaseStream.Position >= _block.EndOffset)
                return default;

            var recordReader = FileParser.GetRecordReader(FileParser.Header.RecordSize);
            return Serializer.Deserialize(recordReader, FileParser);
        }

        internal override void ResetIterator()
        {
            FileParser.BaseStream.Position = _block.StartOffset;
        }
    }
}
