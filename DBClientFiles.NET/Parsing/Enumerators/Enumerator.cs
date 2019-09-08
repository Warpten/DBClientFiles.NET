using System.Collections;
using System.Collections.Generic;
using DBClientFiles.NET.Parsing.Versions;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    /// <summary>
    /// A base implementation of the enumerator used to read the records.
    /// 
    /// This enumerator has no notion of special blocks it would need to handle.
    /// </summary>
    internal abstract class Enumerator<TValue> : IRecordEnumerator<TValue>
    {
        internal BinaryStorageFile<TValue> Parser { get; }

        protected Enumerator(BinaryStorageFile<TValue> owner)
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
            return !EqualityComparer<TValue>.Default.Equals(Current, default);
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

        public virtual Enumerator<TValue> WithCopyTable()
        {
            return Parser.Header.CopyTable.Exists
                ? new CopyTableEnumerator<TValue>(this)
                : this;
        }

        public virtual Enumerator<TValue> WithIndexTable()
        {
            return Parser.Header.IndexTable.Exists
                ? new IndexTableEnumerator<TValue>(this)
                : this;
        }

        public abstract void Skip(int skipCount);
        public abstract TValue ElementAt(int index);
        public abstract TValue ElementAtOrDefault(int index);
        public abstract TValue Last();
    }
}
