using DBClientFiles.NET.Parsing.Binding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DBClientFiles.NET.Parsing.Types
{
    internal sealed class ArrayElementTypeMember : BaseTypeMember
    {
        public override Type Type { get; }
        public override int Cardinality { get; set; }

        public ArrayElementTypeMember(FieldInfo memberInfo, ITypeMember parent) : base(memberInfo, parent)
        {
            Type = parent.Type.GetElementType();

            Cardinality = parent.Cardinality;

            Children = new List<ITypeMember>(Type.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(
                fieldInfo =>
                {
                    return TypeMemberFactory.Create(fieldInfo, this);
                }));
        }

        public ArrayElementTypeMember(PropertyInfo memberInfo, ITypeMember parent) : base(memberInfo, parent)
        {
            Type = parent.Type.GetElementType();

            Cardinality = parent.Cardinality;

            Children = new List<ITypeMember>(Type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(
                propInfo =>
                {
                    return TypeMemberFactory.Create(propInfo, this);
                }));
        }

        public override ExtendedMemberExpression MakeMemberAccess(Expression parent, params Expression[] arguments)
        {
            Debug.Assert(arguments.Length >= 1);

            return new ExtendedMemberExpression(Parent.MakeMemberAccess(parent).Expression, this);
        }

        public override ExtendedMemberExpression MakeMemberAccess(Expression parent)
        {
            throw new InvalidOperationException();
        }
    }
}
