using DBClientFiles.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Parsing.Reflection
{
    internal abstract class Member
    {
        public MemberInfo MemberInfo { get; }
        public MemberTypes MemberType => MemberInfo.MemberType;

        public bool IsField => MemberType == MemberTypes.Field;
        public bool IsProperty => MemberType == MemberTypes.Property;
        public bool IsArray => InternalGetMemberType().IsArray;

        public int Cardinality { get; set; }

        public TypeInfo Type { get; }
        public TypeInfo DeclaringType { get; }

        protected Member(TypeInfo parent, MemberInfo memberInfo)
        {
            DeclaringType = parent;
            MemberInfo = memberInfo;

            Type = parent.GetChildTypeInfo(InternalGetMemberType());
            if (Type.Type.IsArray)
            {
                var marshalAttr = memberInfo.GetCustomAttribute<MarshalAsAttribute>();
                if (marshalAttr != null)
                    Cardinality = marshalAttr.SizeConst;
                else
                {
                    var cardAttr = memberInfo.GetCustomAttribute<CardinalityAttribute>();
                    Cardinality = cardAttr?.SizeConst ?? -1;
                }
            }
        }

        public abstract bool IsReadOnly { get; }

        public abstract Expression MakeAccess(Expression parent);

        protected abstract Type InternalGetMemberType();

        public T GetAttribute<T>() where T : Attribute
        {
            return MemberInfo.GetCustomAttribute<T>();
        }
    }

    internal sealed class Field : Member
    {
        public Field(TypeInfo parent, FieldInfo fieldInfo) : base(parent, fieldInfo)
        {
        }

        protected override Type InternalGetMemberType()
        {
            return ((FieldInfo) MemberInfo).FieldType;
        }

        public override Expression MakeAccess(Expression parent)
        {
            return Expression.MakeMemberAccess(parent, MemberInfo);
        }

        public override bool IsReadOnly => ((FieldInfo) MemberInfo).IsInitOnly;
    }

    internal sealed class Property : Member
    {
        public Property(TypeInfo parent, PropertyInfo fieldInfo) : base(parent, fieldInfo)
        {
        }

        protected override Type InternalGetMemberType()
        {
            return ((PropertyInfo) MemberInfo).PropertyType;
        }

        public override Expression MakeAccess(Expression parent)
        {
            return Expression.MakeMemberAccess(parent, MemberInfo);
        }

        public override bool IsReadOnly => ((PropertyInfo) MemberInfo).GetSetMethod() == null;
    }
}
