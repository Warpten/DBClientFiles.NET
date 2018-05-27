using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBClientFiles.NET.Internals;

namespace DBClientFiles.NET.Collections.Generic
{
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
