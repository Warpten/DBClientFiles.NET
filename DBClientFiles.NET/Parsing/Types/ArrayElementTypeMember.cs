using DBClientFiles.NET.Parsing.Binding;
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace DBClientFiles.NET.Parsing.Types
{
    internal sealed class ArrayElementTypeMember : BaseTypeMember
    {
        public override Type Type { get; }
        public override int Cardinality { get; set; }

        public ArrayElementTypeMember(MemberInfo memberInfo, ITypeMember parent) : base(memberInfo, parent)
        {
            Type = parent.Type.GetElementType();

            Cardinality = parent.Cardinality;
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
