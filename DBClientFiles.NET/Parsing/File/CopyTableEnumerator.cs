using System.Collections.Generic;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.File
{
    /// <summary>
    /// An extension of the enumerator above that implements copy tables.
    /// </summary>
    internal class CopyTableEnumerator<TValue, TSerializer> : Enumerator<TValue, TSerializer>
        where TSerializer : ISerializer<TValue>, new()
    {
        internal CopyTableHandler _copyTableHandler;
        internal List<int> _currentSourceKey;
        internal int _idxTargetKey;

        public CopyTableEnumerator(BinaryFileParser<TValue, TSerializer> owner, CopyTableHandler copyTableHandler) : base(owner)
        {
            _copyTableHandler = copyTableHandler;

            _currentSourceKey = default;
            _idxTargetKey = 0;
        }

        public override void Dispose()
        {
            base.Dispose();
            _copyTableHandler = null;
        }

        public override bool MoveNext()
        {
            if (_currentSourceKey == null || _idxTargetKey == _currentSourceKey.Count)
            {
                var success = base.MoveNext();
                if (!success)
                    return false;

                _currentSourceKey = _copyTableHandler[Serializer.GetKey(in _current)];
                _idxTargetKey = 0;
                return true;
            }

            var newCurrent = Serializer.Clone(in _current);
            Serializer.SetKey(out newCurrent, _currentSourceKey[_idxTargetKey]);
            ++_idxTargetKey;

            _current = newCurrent;

            return true;
        }

        public override void Reset()
        {
            base.Reset();

            _idxTargetKey = 0;
            _currentSourceKey = null;
        }
    }
}
