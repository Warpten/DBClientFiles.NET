using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Internals.Generators;

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
        private IEnumerable<InstanceProxy<T>> _enumerable;

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
            _implementation = new StorageImpl<T>(fileStream, options);
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_enumerable == null)
                throw new ObjectDisposedException("StorageEnumerable<T>");

            return _enumerable.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if (_enumerable == null)
                throw new ObjectDisposedException("StorageEnumerable<T>");

            foreach (var data in _enumerable)
                yield return data.Instance;
        }

        internal IEnumerable<InstanceProxy<T>> Enumerate()
        {
            return _enumerable;
        }
    }
}
