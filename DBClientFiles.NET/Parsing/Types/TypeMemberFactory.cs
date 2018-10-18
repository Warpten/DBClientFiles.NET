using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Exceptions;
using System.Reflection;

namespace DBClientFiles.NET.Parsing.Types
{
    internal static class TypeMemberFactory
    {
        private static ITypeMember Create(PropertyInfo propertyInfo, ITypeMember parent)
        {
            if (propertyInfo.GetGetMethod() == null || propertyInfo.GetSetMethod() == null)
                return null;

            if (propertyInfo.PropertyType.IsArray)
                return CreateArrayMember(propertyInfo, parent, MemberTypes.Property);

            return CreateMember(propertyInfo, parent, MemberTypes.Property);
        }

        private static ITypeMember Create(FieldInfo fieldInfo, ITypeMember parent)
        {
            if (fieldInfo.IsInitOnly)
                return null;

            if (fieldInfo.FieldType.IsArray)
                return CreateArrayMember(fieldInfo, parent, MemberTypes.Field);

            return CreateMember(fieldInfo, parent, MemberTypes.Field);
        }

        public static ITypeMember Create(MemberInfo memberInfo, ITypeMember parent)
        {
            if (memberInfo is FieldInfo fieldInfo)
                return Create(fieldInfo, parent);
            else if (memberInfo is PropertyInfo propInfo)
                return Create(propInfo, parent);
            return null;
        }

        private static ITypeMember CreateArrayMember(MemberInfo memberInfo, ITypeMember parent, MemberTypes memberType)
        {
            if (memberInfo.IsDefined(typeof(IgnoreAttribute)))
                return null;

            var typeMember = new TypeMember(memberInfo, parent);
            if (typeMember.Type.IsArray)
            {
                var arrayMember = new ArrayElementTypeMember(memberInfo, typeMember);
                PopulateChildren(arrayMember, memberType);
                typeMember.Children.Add(arrayMember);
            }
            else
            {
                PopulateChildren(typeMember, memberType);
            }
            return typeMember;
        }

        private static void PopulateChildren(ITypeMember typeMember, MemberTypes memberType)
        {
            foreach (var child in typeMember.Type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (child.MemberType != memberType)
                    continue;

                var childTypeMember = Create(child, typeMember);
                if (childTypeMember != null)
                    typeMember.Children.Add(childTypeMember);
            }
        }

        private static ITypeMember CreateMember(MemberInfo memberInfo, ITypeMember parent, MemberTypes memberType)
        {
            if (memberInfo.IsDefined(typeof(IgnoreAttribute)))
                return null;

            var typeMember = new TypeMember(memberInfo, parent);

            if (typeMember.Type.IsValueType && !typeMember.Type.IsPrimitive && memberInfo is PropertyInfo)
                throw new InvalidTypeException($"Member {memberInfo.Name} defined in {parent.MemberInfo.Name} is a value type. Change to a reference type.");

            foreach (var child in typeMember.Type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (child.MemberType != memberType)
                    continue;

                var childTypeMember = Create(child, typeMember);
                if (childTypeMember != null)
                    typeMember.Children.Add(childTypeMember);
            }

            return typeMember;
        }
    }
}
