using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TypeInfo = DBClientFiles.NET.Parsing.Reflection.TypeInfo;

namespace DBClientFiles.NET.Parsing.Binding
{
    internal class TypeMapper
    {
        public TypeInfo Type { get; }
        public Dictionary<Member, IFileMemberMetadata> Map { get; }

        public TypeMapper(TypeInfo type)
        {
            Type = type;

            Map = new Dictionary<Member, IFileMemberMetadata>();
        }

        public void Resolve(MemberTypes memberType, IEnumerable<IFileMemberMetadata> fileMembers)
        {
            if (!(memberType == MemberTypes.Field || memberType == MemberTypes.Property))
                throw new ArgumentException(nameof(memberType));

            IEnumerable<Member> typeMembers = Type.Members.Flatten(m => m.Type.Members).Where(m => m.MemberType == memberType);
            if (typeMembers == null)
                return;

            using (var typeMembersEnumerator = typeMembers.GetEnumerator())
            {
                foreach (var fileMemberInfo in fileMembers)
                {
                    if (!typeMembersEnumerator.MoveNext())
                        return;

                    var typeMemberType = typeMembersEnumerator.Current.Type.Type;
                    if (typeMemberType.IsArray)
                        typeMemberType = typeMemberType.GetElementType();

                    var structureFieldSize = Math.Max(1, typeMembersEnumerator.Current.Cardinality) * UnsafeCache.SizeOf(typeMemberType);
                    var metaFieldSize = fileMemberInfo.Size * Math.Max(1, fileMemberInfo.Cardinality) / 8;

                    if (structureFieldSize < metaFieldSize)
                        return; // TODO Throw: binary size mismatch

                    Map[typeMembersEnumerator.Current] = fileMemberInfo;
                }

                if (typeMembersEnumerator.MoveNext())
                    return; // TODO: Throw. We have more members declared in code than we need.
            }
        }
    }
}
