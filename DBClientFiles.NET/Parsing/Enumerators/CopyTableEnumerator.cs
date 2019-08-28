using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    internal class CopyTableEnumerator<TParser, TValue, TSerializer> : DecoratingEnumerator<TParser, TValue, TSerializer>
        where TSerializer : ISerializer<TValue>, new()
        where TParser : BinaryFileParser<TValue, TSerializer>
    {
        private readonly CopyTableHandler _blockHandler;

        private IEnumerator<int> _currentCopyIndex;

        private TValue _currentCopySource;
        private Func<TValue> _instanceFactory { get; }

        public CopyTableEnumerator(Enumerator<TParser, TValue, TSerializer> impl) : base(impl)
        {
            if (Parser.Header.CopyTable.Exists)
            {
                _blockHandler = Parser.FindSegmentHandler<CopyTableHandler>(SegmentIdentifier.CopyTable);
                Debug.Assert(_blockHandler != null, "Block handler missing for copy table");

                _instanceFactory = () =>
                {
                    // If no copy left, return self
                    if (_currentCopyIndex == null || !_currentCopyIndex.MoveNext())
                    {
                        // Store a reference to the original uncopied object
                        var originalObject = _currentCopySource;

                        // Prefetch new instance
                        _currentCopySource = base.ObtainCurrent();
                        if (_currentCopySource != default)
                        {
                            // Try to get copy table
                            if (_blockHandler.TryGetValue(Serializer.GetRecordIndex(in _currentCopySource), out var copyKeys))
                                _currentCopyIndex = copyKeys.GetEnumerator();
                            else
                                _currentCopyIndex = null;
                        }

                        return originalObject;
                    }

                    var copiedSource = Serializer.Clone(in _currentCopySource);
                    Serializer.SetRecordIndex(out copiedSource, _currentCopyIndex.Current);
                    return copiedSource;
                };
            }
            else
            {
                _instanceFactory = base.ObtainCurrent;
            } 

        }

        internal override TValue ObtainCurrent()
        {
            return _instanceFactory();
        }

        internal override void ResetIterator()
        {
            throw new NotImplementedException();
        }

        public override Enumerator<TParser, TValue, TSerializer> WithCopyTable()
        {
            return this;
        }
    }
}
