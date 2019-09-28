using DBClientFiles.NET.Parsing.Runtime.Serialization;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    internal class CopyTableEnumerator<TValue> : DecoratingEnumerator<TValue>
        where TValue : notnull
    {
        private IEnumerator<int> _currentCopyIndex;

        private TValue _currentInstance;

        private Func<bool, TValue> InstanceFactory { get; }

        private readonly CopyTableHandler _blockHandler;
        private readonly RuntimeCloner<TValue> _cloningFactory;
        private readonly RecordKeyAccessor<TValue> _keyAccessor;

        public CopyTableEnumerator(Enumerator<TValue> impl, RecordKeyAccessor<TValue> keyAccessor) : base(impl)
        {
            _keyAccessor = keyAccessor;
            _currentInstance = default;
            _currentCopyIndex = default;
            _blockHandler = default;

            if (Parser.Header.CopyTable.Exists)
            {
                _blockHandler = Parser.FindSegmentHandler<CopyTableHandler>(SegmentIdentifier.CopyTable);
                Debug.Assert(_blockHandler != null, "Block handler missing for copy table");

                _cloningFactory = new RuntimeCloner<TValue>(Parser.Type, Parser.Options.TokenType);

                InstanceFactory = forceReloadBase =>
                {
                    // Read an instance if one exists or if we're forced to
                    if (forceReloadBase || EqualityComparer<TValue>.Default.Equals(_currentInstance, default))
                    {
                        _currentInstance = base.ObtainCurrent();

                        // If we got default(TValue) from the underlying implementation we really are done
                        if (EqualityComparer<TValue>.Default.Equals(_currentInstance, default))
                            return default!;
                    }

                    // If no copy table is found, prefetch it, and return the instance that will be cloned
                    if (_currentCopyIndex == null)
                    {
                        // Prepare copy table
                        if (_blockHandler.TryGetValue(_keyAccessor.GetRecordKey(in _currentInstance), out var copyKeys))
                            _currentCopyIndex = copyKeys.GetEnumerator();

                        return _currentInstance;
                    }
                    else if (_currentCopyIndex.MoveNext())
                    {
                        // If the copy table is not done, clone and change index
                        _cloningFactory.Clone(in _currentInstance, out var copiedInstance);
                        _keyAccessor.SetRecordKey(out copiedInstance, _currentCopyIndex.Current);

                        return copiedInstance;
                    }
                    else
                    {
                        // We were unable to move next in the copy table, which means we are done with the current record
                        // and its copies. Re-setup the copy table check.
                        _currentCopyIndex = default!;
                        
                        // Call ourselves again to initialize everything for the next record.
                        _currentInstance = InstanceFactory(true);
                        return _currentInstance;
                    }
                };
            }
            else
            {
                InstanceFactory = _ => base.ObtainCurrent();
            }
        }

        internal override TValue ObtainCurrent()
        {
            return InstanceFactory(false);
        }

        public override void Reset()
        {
            _currentInstance = default!;
            _currentCopyIndex = default!;

            base.Reset();
        }

        public override Enumerator<TValue> WithCopyTable()
        {
            return this;
        }

        public override TValue Last()
        {
            var lastSource = base.Last();
            if (_blockHandler != null && !_blockHandler.TryGetValue(_keyAccessor.GetRecordKey(in _currentInstance), out var copyKeys))
            {
                var lastCopyKey = copyKeys.Last();
                _keyAccessor.SetRecordKey(out lastSource, lastCopyKey);
            }

            return lastSource;
        }

        // TODO: Optimize Skip and ElementAt(OrDefault)
    }
}
