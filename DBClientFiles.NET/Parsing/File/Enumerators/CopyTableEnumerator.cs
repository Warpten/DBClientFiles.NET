using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DBClientFiles.NET.Parsing.File.Enumerators
{
    internal class CopyTableEnumerator<TValue, TSerializer> : DecoratingEnumerator<TValue, TSerializer>
        where TSerializer : ISerializer<TValue>, new()
    {
        private readonly CopyTableHandler _blockHandler;

        private IEnumerator<int> _currentCopyIndex;

        private TValue _currentCopySource;
        private Func<TValue> _instanceFactory { get; }

        public CopyTableEnumerator(Enumerator<TValue, TSerializer> impl) : base(impl)
        {
            if (Parser.Header.CopyTable.Exists)
            {
                _blockHandler = Parser.FindBlockHandler<CopyTableHandler>(BlockIdentifier.CopyTable);
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

        public override Enumerator<TValue, TSerializer> WithCopyTable()
        {
            return this;
        }
    }
}
