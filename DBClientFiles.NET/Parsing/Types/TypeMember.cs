using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Parsing.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.Types
{
    internal sealed class TypeMember : BaseTypeMember
    {
        public TypeMember(PropertyInfo propInfo, ITypeMember parent) : base(propInfo, parent)
        {
            Type = propInfo.PropertyType;
            if (Type.IsArray)
            {
                var marshalAttribute = propInfo.GetCustomAttribute<MarshalAsAttribute>();
                if (marshalAttribute != null)
                    Cardinality = marshalAttribute.SizeConst;
                else
                {
                    var cardinalityAttribute = propInfo.GetCustomAttribute<CardinalityAttribute>();
                    if (cardinalityAttribute == null)
                        Cardinality = -1;
                    else
                        Cardinality = cardinalityAttribute.SizeConst;
                }
            }
            else
                Cardinality = 0;

            if (!Type.IsArray)
                Children = new List<ITypeMember>();
            else
            {
                Children = new List<ITypeMember>(Type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(
                    childPropInfo =>
                    {
                        return TypeMemberFactory.Create(childPropInfo, this);
                    }));
            }
        }

        public TypeMember(FieldInfo fieldInfo, ITypeMember parent) : base(fieldInfo, parent)
        {
            Type = fieldInfo.FieldType;

            if (Type.IsArray)
            {
                var marshalAttribute = fieldInfo.GetCustomAttribute<MarshalAsAttribute>();
                if (marshalAttribute != null)
                    Cardinality = marshalAttribute.SizeConst;
                else
                {
                    var cardinalityAttribute = fieldInfo.GetCustomAttribute<CardinalityAttribute>();
                    if (cardinalityAttribute == null)
                        Cardinality = -1;
                    else
                        Cardinality = cardinalityAttribute.SizeConst;
                }
            }
            else
                Cardinality = 0;

            if (!Type.IsArray)
                Children = new List<ITypeMember>();
            else
            {
                Children = new List<ITypeMember>(Type.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(
                    childFieldInfo =>
                    {
                        return TypeMemberFactory.Create(childFieldInfo, this);
                    }));
            }
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
