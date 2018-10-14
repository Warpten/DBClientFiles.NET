using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Types;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    internal sealed class Serializer<T> : BaseSerializer<T>
    {
        public Serializer(StorageOptions options, TypeInfo typeInfo) : base(options, typeInfo)
        {
        }

        protected override IMemberSerializer GetMemberSerializer(ITypeMember memberInfo)
        {
            return new MemberSerializer(memberInfo);
        }
    }
}
