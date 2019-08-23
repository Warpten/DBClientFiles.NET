using System.Collections;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.File.Enumerators
{
    /// <summary>
    /// A base implementation of the enumerator used to read the records.
    /// 
    /// This enumerator has no notion of special blocks it would need to handle.
    /// </summary>
    internal abstract partial class Enumerator<TValue, TSerializer> : IEnumerator<TValue>, IEnumerator
        where TSerializer : ISerializer<TValue>, new()
    {
        internal BinaryFileParser<TValue, TSerializer> Parser { get; }

        protected TSerializer Serializer => Parser.Serializer;

        public Enumerator(BinaryFileParser<TValue, TSerializer> owner)
        {
            Parser = owner;
            Current = default;
        }

        #region IEnumerator
        public TValue Current { get; private set; }

        // Explicit implementation of the non-generic IEnumerator interface.
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            Current = ObtainCurrent();
            return Current != default;
        }

        public void Reset()
        {
            Current = default;
            ResetIterator();
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
        }
        #endregion

        internal abstract void ResetIterator();

        internal abstract TValue ObtainCurrent();

        public virtual Enumerator<TValue, TSerializer> WithCopyTable()
        {
            return Parser.Header.CopyTable.Exists
                ? new CopyTableEnumerator<TValue, TSerializer>(this)
                : this;
        }

        public virtual Enumerator<TValue, TSerializer> WithIndexTable()
        {
            return Parser.Header.IndexTable.Exists
                ? new IndexTableEnumerator<TValue, TSerializer>(this)
                : this;
        }
    }
}
