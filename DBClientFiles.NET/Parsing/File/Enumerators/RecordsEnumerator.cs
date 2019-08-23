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
            _block = Parser.FindBlock(BlockIdentifier.Records);
            Debug.Assert(_block != null, "Records block missing in enumerator");

            Parser.BaseStream.Position = _block.StartOffset;
        }

        internal override TValue ObtainCurrent()
        {
            if (Parser.BaseStream.Position >= _block.EndOffset)
                return default;

            var recordReader = Parser.GetRecordReader(Parser.Header.RecordSize);
            return Serializer.Deserialize(recordReader, Parser);
        }

        internal override void ResetIterator()
        {
            Parser.BaseStream.Position = _block.StartOffset;
        }
    }
}
