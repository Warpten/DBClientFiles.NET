using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Exceptions;
using System.Reflection;

namespace DBClientFiles.NET.Parsing.Types
{
    internal static class TypeMemberFactory
    {
        public static ITypeMember Create(PropertyInfo propertyInfo, ITypeMember parent)
        {
            if (propertyInfo.IsDefined(typeof(IgnoreAttribute)) || propertyInfo.GetGetMethod() == null || propertyInfo.GetSetMethod() == null)
                return null;

            var member = new TypeMember(propertyInfo, parent);
            if (propertyInfo.PropertyType.IsArray)
            {
                var arrayElementMember = new ArrayElementTypeMember(propertyInfo, member);
                member.Children.Add(arrayElementMember);
            }

            return member;
        }

        public static ITypeMember Create(FieldInfo fieldInfo, ITypeMember parent)
        {
            if (fieldInfo.IsDefined(typeof(IgnoreAttribute)) || fieldInfo.IsInitOnly)
                return null;

            var member = new TypeMember(fieldInfo, parent);
            if (fieldInfo.FieldType.IsArray)
            {
                var arrayElementMember = new ArrayElementTypeMember(fieldInfo, member);
                member.Children.Add(arrayElementMember);
            }

            return member;
        }
    }
}
