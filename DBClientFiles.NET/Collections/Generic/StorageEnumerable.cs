using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Collections.Generic
{
    /// <summary>
    /// An enumerable storage representation of dbc and db2 files.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public class StorageEnumerable<T> : IStorage, IEnumerable<T>
        where T : class, new()
    {
        #region IStorage
        public Signatures Signature { get; }
        public uint TableHash { get; }
        public uint LayoutHash { get; }
        #endregion

        private StorageImpl<T> _implementation;
        private IEnumerable<T> _enumerable;

        public Dictionary<long, string> StringTable { get; } = new Dictionary<long, string>();

        public StorageEnumerable(Stream fileStream) : this(fileStream, StorageOptions.Default)
        {
        }

        ~StorageEnumerable()
        {
            _enumerable = null;
            _implementation.Dispose();
        }

        public StorageEnumerable(Stream fileStream, StorageOptions options)
        {
            if (options.LoadMask.HasFlag(LoadMask.StringTable))
                StringTable = new Dictionary<long, string>();

            _implementation = new StorageImpl<T>(fileStream, options);
            _implementation.OnStringTableEntry += StringTable.Add;
            _implementation.InitializeHeaderInfo();

            //! Slow.
            var indexMember = _implementation.Members.IndexMember;
            var initializerMethod = _implementation.GetType().GetMethod("InitializeFileReader", Type.EmptyTypes).MakeGenericMethod(indexMember.Type);
            initializerMethod.Invoke(_implementation, null);

            // Back to non-generic, we got the proper type now.
            _implementation.PrepareMemberInfo();

            _enumerable = _implementation.Enumerate();

            Signature = _implementation.Header.Signature;
            TableHash = _implementation.Header.TableHash;
            LayoutHash = _implementation.Header.LayoutHash;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_enumerable == null)
                throw new ObjectDisposedException("StorageEnumerable<T>");

            return _enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_enumerable == null)
                throw new ObjectDisposedException("StorageEnumerable<T>");

            return _enumerable.GetEnumerator();
        }
    }
}
