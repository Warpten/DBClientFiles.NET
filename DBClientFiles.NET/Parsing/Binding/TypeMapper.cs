using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TypeToken = DBClientFiles.NET.Parsing.Reflection.TypeToken;

namespace DBClientFiles.NET.Parsing.Binding
{
    internal class TypeMapper
    {
        public TypeToken Type { get; }
        public Dictionary<MemberToken, IFileMemberMetadata> Map { get; }

        public TypeMapper(TypeToken type)
        {
            Type = type;

            Map = new Dictionary<MemberToken, IFileMemberMetadata>();
        }

        public void Resolve(TypeTokenType memberType, IEnumerable<IFileMemberMetadata> fileMembers)
        {
            if (!(memberType == TypeTokenType.Field || memberType == TypeTokenType.Property))
                throw new ArgumentException(nameof(memberType));

            IEnumerable<MemberToken> typeMembers = Type.Members.Flatten(m => m.TypeToken.Members).Where(m => m.MemberType == memberType);
            if (typeMembers == null)
                return;

            using (var typeMembersEnumerator = typeMembers.GetEnumerator())
            {
                foreach (var fileMemberInfo in fileMembers)
                {
                    if (!typeMembersEnumerator.MoveNext())
                        return;

                    var typeMemberType = typeMembersEnumerator.Current.TypeToken.Type;
                    if (typeMemberType.IsArray)
                        typeMemberType = typeMemberType.GetElementType();

                    // TODO: FIXME
                    var structureFieldSize = 1; // Math.Max(1, typeMembersEnumerator.Current.Cardinality) * UnsafeCache.SizeOf(typeMemberType);
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
