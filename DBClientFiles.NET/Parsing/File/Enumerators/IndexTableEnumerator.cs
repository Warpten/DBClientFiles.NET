using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DBClientFiles.NET.Parsing.File.Enumerators
{
    internal class IndexTableEnumerator<TValue, TSerializer> : DecoratingEnumerator<TValue, TSerializer>
        where TSerializer : ISerializer<TValue>, new()
    {
        private readonly IndexTableHandler _blockHandler;
        private int _cursor;

        public IndexTableEnumerator(Enumerator<TValue, TSerializer> impl) : base(impl)
        {
            _blockHandler = Parser.FindBlockHandler<IndexTableHandler>(BlockIdentifier.IndexTable);
            Debug.Assert(_blockHandler != null, "Block handler missing for index table");
            _cursor = 0;
        }

        internal override TValue ObtainCurrent()
        {
            var instance = base.ObtainCurrent();
            if (instance == default)
                return default;

            Serializer.SetRecordIndex(out instance, _blockHandler[_cursor]);
            ++_cursor;
            return instance;
        }

        internal override void ResetIterator()
        {
            _cursor = 0;
        }

        public override Enumerator<TValue, TSerializer> WithIndexTable()
        {
            return this;
        }
    }
}
