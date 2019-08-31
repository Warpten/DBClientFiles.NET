using System.Collections;
using System.Collections.Generic;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Versions;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    /// <summary>
    /// A base implementation of the enumerator used to read the records.
    /// 
    /// This enumerator has no notion of special blocks it would need to handle.
    /// </summary>
    internal abstract partial class Enumerator<TParser, TValue, TSerializer> : IEnumerator<TValue>, IEnumerator
        where TSerializer : ISerializer<TValue>, new()
        where TParser : BinaryFileParser<TValue, TSerializer>
    {
        internal TParser Parser { get; }

        protected TSerializer Serializer => Parser.Serializer;

        public Enumerator(TParser owner)
        {
            Parser = owner;
            Current = default;
        }

        #region IEnumerator
        public TValue Current { get; private set; }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            Current = ObtainCurrent();
            return Current != default;
        }

        public virtual void Reset()
        {
            Current = default;
        }
        #endregion

        #region IDisposable
        public virtual void Dispose()
        {
        }
        #endregion

        internal abstract TValue ObtainCurrent();

        public virtual Enumerator<TParser, TValue, TSerializer> WithCopyTable()
        {
            return Parser.Header.CopyTable.Exists
                ? new CopyTableEnumerator<TParser, TValue, TSerializer>(this)
                : this;
        }

        public virtual Enumerator<TParser, TValue, TSerializer> WithIndexTable()
        {
            return Parser.Header.IndexTable.Exists
                ? new IndexTableEnumerator<TParser, TValue, TSerializer>(this)
                : this;
        }
    }
}
