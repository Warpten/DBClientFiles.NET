using DBClientFiles.NET.Internals.Binding;
using DBClientFiles.NET.Parsing.Types;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

using TypeInfo = DBClientFiles.NET.Parsing.Types.TypeInfo;

namespace DBClientFiles.NET.Parsing.Binding
{
    internal class TypeMapper<T>
    {
        public TypeInfo Type { get; }
        public Dictionary<IMemberMetadata, ITypeMember> Map { get; }

        public TypeMapper(MemberTypes memberType, IEnumerable<IMemberMetadata> fileMembers)
        {
            Type = TypeInfo.Create<T>();

            Map = new Dictionary<IMemberMetadata, ITypeMember>();

            if (!(memberType == MemberTypes.Field || memberType == MemberTypes.Property))
                throw new ArgumentException(nameof(memberType));

            IEnumerable<ITypeMember> typeMembers = Type.EnumerateFlat(memberType);
            if (typeMembers == null)
                return;

            using (var typeMembersEnumerator = typeMembers.GetEnumerator())
            {
                foreach (var fileMemberInfo in fileMembers)
                {
                    // TODO: Throw. We have less members in code than we need.
                    if (!typeMembersEnumerator.MoveNext())
                        return;

                    var typeMemberType = typeMembersEnumerator.Current.Type;
                    if (typeMemberType.IsArray)
                        typeMemberType = typeMemberType.GetElementType();

                    var structureFieldSize = Math.Max(1, typeMembersEnumerator.Current.Cardinality) * UnsafeCache.SizeOf(typeMemberType);
                    var metaFieldSize = fileMemberInfo.GetElementBitSize() * Math.Max(1, fileMemberInfo.Cardinality) * 8;

                    if (structureFieldSize < metaFieldSize)
                        return; // TODO Throw: binary size mismatch

                    Map[fileMemberInfo] = typeMembersEnumerator.Current;

                    // Update cardinality
                    if (fileMemberInfo.Cardinality > -1)
                        typeMembersEnumerator.Current.Cardinality = fileMemberInfo.Cardinality;
                }

                if (typeMembersEnumerator.MoveNext())
                    return; // TODO: Throw. We have more members declared in code than we need.
            }
        }
    }
}
