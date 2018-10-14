using DBClientFiles.NET.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DBClientFiles.NET.Parsing.Types
{
    internal sealed class TypeInfo
    {
        public static TypeInfo Create<T>()
        {
            return new TypeInfo(typeof(T));
        }

        public Type Type { get; }

        private List<ITypeMember> _fields = new List<ITypeMember>();
        private List<ITypeMember> _properties = new List<ITypeMember>();

        public IEnumerable<ITypeMember> Fields => _fields;
        public IEnumerable<ITypeMember> Properties => _properties;

        private TypeInfo(Type rootType)
        {
            Type = rootType;

            var fieldIndex = 0;
            var propertyIndex = 0;

            foreach (var memberInfo in rootType.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (memberInfo is FieldInfo fieldInfo)
                {
                    var typeMemberInfo = TypeMemberFactory.Create(fieldInfo, null);
                    if (typeMemberInfo == null)
                        continue;

                    _fields.Add(typeMemberInfo);
                    fieldIndex += Math.Max(1, typeMemberInfo.Children.Count);
                }
                else if (memberInfo is PropertyInfo propertyInfo)
                {
                    var typeMemberInfo = TypeMemberFactory.Create(propertyInfo, null);
                    if (typeMemberInfo == null)
                        continue;

                    _properties.Add(typeMemberInfo);
                    propertyIndex += Math.Max(1, typeMemberInfo.Children.Count);
                }
            }
        }

        public IEnumerable<ITypeMember> Enumerate(MemberTypes memberType)
        {
            if (memberType == MemberTypes.Field)
                return _fields;
            else if (memberType == MemberTypes.Property)
                return _properties;

            return null;
        }

        public IEnumerable<ITypeMember> EnumerateFlat(MemberTypes memberType)
        {
            if (memberType == MemberTypes.Field)
                return Flatten(_fields);
            else if (memberType == MemberTypes.Property)
                return Flatten(_properties);

            return null;
        }

        private IEnumerable<ITypeMember> Flatten(IEnumerable<ITypeMember> members)
        {
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
