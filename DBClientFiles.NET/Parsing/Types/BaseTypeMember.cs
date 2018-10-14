using DBClientFiles.NET.Parsing.Binding;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DBClientFiles.NET.Parsing.Types
{
    internal abstract class BaseTypeMember : ITypeMember
    {
        protected BaseTypeMember(MemberInfo memberInfo, ITypeMember parent)
        {
            MemberInfo = memberInfo;
            Parent = parent;

            Children = new List<ITypeMember>();
        }

        public MemberInfo MemberInfo { get; }
        public ITypeMember Parent { get; }
        public abstract Type Type { get; }

        public List<ITypeMember> Children { get; }

        public bool IsArray => Type.IsArray;

        public abstract int Cardinality { get; set; }

        public abstract ExtendedMemberExpression MakeMemberAccess(Expression parent, params Expression[] arguments);
        public abstract ExtendedMemberExpression MakeMemberAccess(Expression parent);
    }
}
