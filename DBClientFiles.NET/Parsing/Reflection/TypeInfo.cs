using DBClientFiles.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.Reflection
{
    internal sealed class TypeInfo
    {
        public Type Type { get; }

        public TypeInfo ElementTypeInfo { get; }

        private Dictionary<Type, TypeInfo> _declaredTypes;
        private List<Member> _fields;

        public IEnumerable<TypeInfo> DeclaredTypes => _declaredTypes.Values;
        public IList<Member> Members => _fields;

        public IEnumerable<Member> Fields
            => Members.Where(m => m.IsField);

        public IEnumerable<Member> Properties
            => Members.Where(m => m.IsProperty);

        public bool IsClass => Type.IsClass;

        public TypeInfo(Type type) : this(type, null)
        {

        }

        private TypeInfo(Type type, Dictionary<Type, TypeInfo> declaredParentTypes)
        {
            _declaredTypes = declaredParentTypes ?? new Dictionary<Type, TypeInfo>();
            if (!_declaredTypes.ContainsKey(type))
                _declaredTypes[type] = this;

            _fields = new List<Member>();

            Type = type;
            if (type.IsArray)
                ElementTypeInfo = GetChildTypeInfo(type.GetElementType());

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; ++i)
            {
                ref readonly var fieldInfo = ref fields[i];
                GetChildTypeInfo(fieldInfo.FieldType);

                var fieldType = new Field(this, fieldInfo);
                _fields.Add(fieldType);
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < properties.Length; i++)
            {
                ref readonly var propInfo = ref properties[i];
                GetChildTypeInfo(propInfo.PropertyType);

                _fields.Add(new Property(this, propInfo));
            }
        }

        public Member GetMemberByIndex(int index, MemberTypes memberType)
        {
            foreach (var child in Members)
            {
                if (child.MemberType != memberType)
                    continue;

                if (index == 0)
                    return child;

                --index;
            }

            return null;
        }

        public Member FindChild(MemberInfo reflectionInfo)
        {
            foreach (var child in Members)
                if (child.MemberInfo == reflectionInfo)
                    return child;

            return null;
        }

        public T GetAttribute<T>() where T : Attribute
        {
            return Type.GetCustomAttribute<T>();
        }

        public TypeInfo GetChildTypeInfo(Type type)
        {
            if (_declaredTypes.TryGetValue(type, out var typeInfo))
                return typeInfo;

            _declaredTypes[type] = new TypeInfo(type, _declaredTypes);
            return _declaredTypes[type];
        }
    }
}
