using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Parsing.Binding;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.Types
{
    internal sealed class TypeMember : BaseTypeMember
    {
        public TypeMember(MemberInfo memberInfo, ITypeMember parent) : base(memberInfo, parent)
        {
            if (memberInfo is PropertyInfo propInfo)
                Type = propInfo.PropertyType;
            else if (memberInfo is FieldInfo fieldInfo)
                Type = fieldInfo.FieldType;

            if (Type.IsArray)
            {
                var marshalAttribute = memberInfo.GetCustomAttribute<MarshalAsAttribute>();
                if (marshalAttribute != null)
                    Cardinality = marshalAttribute.SizeConst;
                else
                {
                    var cardinalityAttribute = memberInfo.GetCustomAttribute<CardinalityAttribute>();
                    if (cardinalityAttribute == null)
                        Cardinality = -1;
                    else
                        Cardinality = cardinalityAttribute.SizeConst;
                }
            }
            else
                Cardinality = 0;
        }

        public override Type Type { get; }

        public override int Cardinality { get; set; }

        public override ExtendedMemberExpression MakeMemberAccess(Expression parent)
        {
            return new ExtendedMemberExpression(parent, this);
        }

        public override ExtendedMemberExpression MakeMemberAccess(Expression parent, params Expression[] arguments)
        {
            throw new InvalidOperationException();
        }
    }
}
