using System.Collections;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Internals;

namespace DBClientFiles.NET.Collections.Generic
{
    /// <summary>
    /// An enumerable storage representation of dbc and db2 files.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public class StorageEnumerable<T> : StorageBase<T>, IEnumerable<T>
        where T : class, new()
    {
        private IEnumerable<T> _enumerable;

        public StorageEnumerable(Stream fileStream) : this(fileStream, StorageOptions.Default)
        {
        }

        public StorageEnumerable(Stream fileStream, StorageOptions options)
        {
            FromStream(fileStream, options);
            LoadRecords();
        }

        internal override void LoadRecords(IReader<T> reader)
        {
            _enumerable = reader.ReadRecords();
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }
    }
}
