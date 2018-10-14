using DBClientFiles.NET.Parsing.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Parsing.Types
{
    internal interface ITypeMember
    {
        MemberInfo MemberInfo { get; }
        ITypeMember Parent { get; }
        Type Type { get; }

        List<ITypeMember> Children { get; }

        ExtendedMemberExpression MakeMemberAccess(Expression parent, params Expression[] arguments);
        ExtendedMemberExpression MakeMemberAccess(Expression parent);

        bool IsArray { get; }

        /// <summary>
        /// Cardinality retrieved from either <see cref="MarshalAsAttribute"/> or <see cref="CardinalityAttribute"/>.
        ///
        /// For non-array members this is zero.
        /// </summary>
        int Cardinality { get; set; }
    }
}
