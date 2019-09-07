using System.Collections;
using System.Collections.Generic;
using DBClientFiles.NET.Parsing.Versions;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    internal interface IRecordEnumerator<T> : IEnumerator<T>
    {
        void Skip(int skipCount);

        T ElementAt(int index);

        T ElementAtOrDefault(int index);
    }

    /// <summary>
    /// A base implementation of the enumerator used to read the records.
    /// 
    /// This enumerator has no notion of special blocks it would need to handle.
    /// </summary>
    internal abstract class Enumerator<TParser, TValue> : IRecordEnumerator<TValue>
        where TParser : BinaryStorageFile<TValue>
    {
        internal TParser Parser { get; }

        protected Enumerator(TParser owner)
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

        public virtual Enumerator<TParser, TValue> WithCopyTable()
        {
            return Parser.Header.CopyTable.Exists
                ? new CopyTableEnumerator<TParser, TValue>(this)
                : this;
        }

        public virtual Enumerator<TParser, TValue> WithIndexTable()
        {
            return Parser.Header.IndexTable.Exists
                ? new IndexTableEnumerator<TParser, TValue>(this)
                : this;
        }

        public abstract void Skip(int skipCount);
        public abstract TValue ElementAt(int index);
        public abstract TValue ElementAtOrDefault(int index);
    }
}
