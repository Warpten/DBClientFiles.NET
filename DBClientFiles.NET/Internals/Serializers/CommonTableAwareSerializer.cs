using DBClientFiles.NET.Internals.Versions;
using System;
using System.IO;
using System.Reflection;

namespace DBClientFiles.NET.Internals.Serializers
{
    internal class CommonTableAwareSerializer<TValue> : LegacySerializer<TValue>
    {
        public CommonTableAwareSerializer(BaseReader storage) : base(storage) { }

        protected override bool CanSerializeMember(int memberIndex, MemberInfo memberInfo)
        {
            if (Storage.CommonTable.ColumnCount)
            return base.CanSerializeMember(memberIndex, memberInfo);
        }
    }

    internal class CommonTableAwareSerializer<TKey, TValue> : CommonTableAwareSerializer<TValue>
    {
        public CommonTableAwareSerializer(BaseReader storage) : base(storage) { }

        private Func<BaseReader, BinaryReader, TValue> _structureDeserializer;

        public override TValue Deserialize(BinaryReader reader)
        {
            if (_structureDeserializer == null)
            {
                if (Storage.CommonTable == null)
                    throw new InvalidOperationException("Miising common table for deserialization");
            }

            return _structureDeserializer(Storage, reader);
        }
    }
}
