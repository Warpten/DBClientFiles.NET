using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DBClientFiles.NET.Parsing.Enumerators;

namespace DBClientFiles.NET.Collections.Generic.Internal
{
    internal class SkippingStorageEnumerable<T> : IRecordEnumerable<T>
    {
        private readonly StorageEnumerable<T> _storageEnumerable;
        private readonly IRecordEnumerator<T> _enumerator;

        private readonly int _skipCount;

        public SkippingStorageEnumerable(StorageEnumerable<T> storageEnumerable, int amount)
        {
            _storageEnumerable = storageEnumerable;

            _enumerator = (IRecordEnumerator<T>) storageEnumerable.GetEnumerator();
            _enumerator.Skip(amount);

            _skipCount = amount;
        }

        public void Dispose()
        {
            _enumerator.Dispose();
            _storageEnumerable.Dispose();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IRecordEnumerable<T> Skip(int skipCount)
            => new SkippingStorageEnumerable<T>(_storageEnumerable, _skipCount + skipCount);
    
        public T ElementAt(int offset) => _enumerator.ElementAt(_skipCount + offset);

        public T ElementAtOrDefault(int offset) => _enumerator.ElementAtOrDefault(_skipCount + offset);

        public T Last() => _enumerator.Last();
    }
}
