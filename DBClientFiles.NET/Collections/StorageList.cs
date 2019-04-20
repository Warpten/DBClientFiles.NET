using DBClientFiles.NET.Types;
using System.Collections.Generic;
using System.IO;

namespace DBClientFiles.NET.Collections
{
    public sealed class StorageList
    {
        private IDictionary<uint, Record> _storage;

        private StorageOptions _options;
        public ref readonly StorageOptions Options => ref _options;

        public StorageList(DynamicStructure structure, in StorageOptions options, Stream dataStream)
        {
            _options = options;

            _storage = new Dictionary<uint, Record>();
        }

        public Record this[uint index]
            => _storage[index];

        public IEnumerable<RecordField> this[string memberName]
        {
            get
            {
                foreach (var kv in _storage.Values)
                    yield return kv[memberName];
            }
        }
    }
}
