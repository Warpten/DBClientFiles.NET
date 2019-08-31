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

        private TValue _currentInstance;
        private Func<bool, TValue> _instanceFactory { get; }

        public CopyTableEnumerator(Enumerator<TParser, TValue, TSerializer> impl) : base(impl)
        {
            if (Parser.Header.CopyTable.Exists)
            {
                _blockHandler = Parser.FindSegmentHandler<CopyTableHandler>(SegmentIdentifier.CopyTable);
                Debug.Assert(_blockHandler != null, "Block handler missing for copy table");

                _instanceFactory = (bool forceReloadBase) =>
                {
                    // Read an instance if one exists or if we're forced to
                    if (forceReloadBase || EqualityComparer<TValue>.Default.Equals(_currentInstance, default))
                    {
                        _currentInstance = base.ObtainCurrent();

                        // If we got default(TValue) from the underlying implementation we really are done
                        if (EqualityComparer<TValue>.Default.Equals(_currentInstance, default))
                            return default;
                    }

                    // If no copy table is found, prefetch it, and return the instance that will be cloned
                    if (_currentCopyIndex == null)
                    {
                        // Prepare copy table
                        if (_blockHandler.TryGetValue(Serializer.GetRecordIndex(in _currentInstance), out var copyKeys))
                            _currentCopyIndex = copyKeys.GetEnumerator();

                        return _currentInstance;
                    }
                    else if (_currentCopyIndex.MoveNext())
                    {
                        // If the copy table is not done, clone and change index
                        var copiedInstance = Serializer.Clone(in _currentInstance);
                        Serializer.SetRecordIndex(out copiedInstance, _currentCopyIndex.Current);

                        return copiedInstance;
                    }
                    else
                    {
                        // We were unable to move next in the copy table, which means we are done with the current record
                        // and its copies. Resetup the copy table check.
                        _currentCopyIndex = null;
                        
                        // Call ourselves again to initialize everything for the next record.
                        _currentInstance = _instanceFactory(true);
                        return _currentInstance;
                    }
                };
            }
            else
            {
                _instanceFactory = _ => base.ObtainCurrent();
            } 

        }

        internal override TValue ObtainCurrent()
        {
            return _instanceFactory(false);
        }

        public override void Reset()
        {
            _currentInstance = default;
            _currentCopyIndex = null;

            base.Reset();
        }

        public override Enumerator<TParser, TValue, TSerializer> WithCopyTable()
        {
            return this;
        }
    }
}
