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
        internal BinaryFileParser<TValue, TSerializer> FileParser { get; private set; }

        protected TSerializer Serializer => FileParser.Serializer;

        public Enumerator(BinaryFileParser<TValue, TSerializer> owner)
        {
            FileParser = owner;
            owner.BaseStream.Position = 0;

            owner.Before(ParsingStep.Segments);

            var head = owner.Head;
            while (head != null)
            {
                if (!head.ReadBlock(owner))
                    owner.BaseStream.Seek(head.Length, SeekOrigin.Current);

                head = head.Next;
            }

            owner.After(ParsingStep.Segments);

            // Segments have been processed, it's now time to initialize the deserializer.
            Serializer.Initialize(owner);

            Current = default;
        }

        #region IEnumerator
        public TValue Current { get; private set; }

        // Explicit implementation of the non-generic IEnumerator interface.
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            Current = ObtainCurrent();
            return true;
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
            FileParser = null;
        }
        #endregion

        internal abstract void ResetIterator();

        internal abstract TValue ObtainCurrent();

        public virtual Enumerator<TValue, TSerializer> WithCopyTable()
        {
            return FileParser.Header.CopyTable.Exists
                ? new CopyTableEnumerator<TValue, TSerializer>(this)
                : this;
        }

        public virtual Enumerator<TValue, TSerializer> WithIndexTable()
        {
            return FileParser.Header.IndexTable.Exists
                ? new IndexTableEnumerator<TValue, TSerializer>(this)
                : this;
        }
    }
}
