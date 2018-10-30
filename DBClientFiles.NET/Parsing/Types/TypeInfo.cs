using System;
using System.Collections.Generic;
using System.Reflection;

namespace DBClientFiles.NET.Parsing.Types
{
    internal sealed class TypeInfo
    {
        public static TypeInfo Create<T>(MemberTypes memberType)
        {
            return new TypeInfo(typeof(T), memberType);
        }

        public Type Type { get; }

        public IList<ITypeMember> Members { get; }

        private TypeInfo(Type rootType, MemberTypes memberType)
        {
            Type = rootType;

            if (memberType == MemberTypes.Field)
            {
                var fieldInfos = rootType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                Members = new List<ITypeMember>(fieldInfos.Length);
                for (var i = 0; i < fieldInfos.Length; ++i)
                {
                    var typeMemberInfo = TypeMemberFactory.Create(fieldInfos[i], null);
                    if (typeMemberInfo != null)
                        Members.Add(typeMemberInfo);
                }
            }
            else if (memberType == MemberTypes.Property)
            {
                var propInfos = rootType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Members = new List<ITypeMember>(propInfos.Length);
                for (var i = 0; i < propInfos.Length; ++i)
                {
                    var typeMemberInfo = TypeMemberFactory.Create(propInfos[i], null);
                    if (typeMemberInfo != null)
                        Members.Add(typeMemberInfo);
                }
            }
        }

        public IEnumerable<ITypeMember> EnumerateFlat()
        {
            return Flatten(Members);
        }

        private static IEnumerable<ITypeMember> Flatten(IEnumerable<ITypeMember> members)
        {
            // This is fine as long as the tree isn't too deep for the GC
            // to start promoting top iterators to gen 1 or 2 - which it
            // shoudln't.

            foreach (var member in members)
            {
                if (member.Children.Count == 0)
                    yield return member;
                else
                {
                    var flattenedChildren = Flatten(member.Children);
                    foreach (var flattenedChild in flattenedChildren)
                        yield return flattenedChild;
                }
            }
        }
    }
}
