using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

using TypeToken = DBClientFiles.NET.Parsing.Reflection.TypeToken;

namespace DBClientFiles.NET.Parsing.Binding
{
    internal class TypeMapper
    {
        public TypeToken Type { get; }
        public Dictionary<MemberToken, IMemberMetadata> Map { get; }

        public TypeMapper(TypeToken type)
        {
            Type = type;

            Map = new Dictionary<MemberToken, IMemberMetadata>();
        }

        public void Resolve(TypeTokenType memberType, IList<IMemberMetadata> fileMembers)
        {
            if (!(memberType == TypeTokenType.Field || memberType == TypeTokenType.Property))
                throw new ArgumentException(nameof(memberType));

            // Retrieve every member of the user-defined structure.
            IEnumerable<MemberToken> typeMembers = Type.Members.Flatten(m => m.TypeToken.Members).Where(m => m.MemberType == memberType);
            if (typeMembers == null)
                return;

            // TODO: Improve this algorithm
            // It has tons of shortcomings

            // A. struct Foo { int, float } ... Foo[2];
            //    versus
            //    int A, float B, int C, float D
            //    Easy to handle with WDBC but when A and C (or B and D) have different bitness, then all hell breaks loose
            using (var typeMembersEnumerator = typeMembers.GetEnumerator())
            {
                // Iterate over all the members defined in file.
                foreach (var fileMemberInfo in fileMembers)
                {
                    if (!typeMembersEnumerator.MoveNext())
                        return;

                    var typeMemberType = typeMembersEnumerator.Current.TypeToken.Type;
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
