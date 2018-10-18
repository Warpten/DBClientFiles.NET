using DBClientFiles.NET.Parsing.Types;
using System.Linq.Expressions;
using Expr = System.Linq.Expressions.Expression;

namespace DBClientFiles.NET.Parsing.Binding
{
    /// <summary>
    /// A simple wrapper around <see cref="MemberExpression"/> and <see cref="ExtendedMemberInfo"/>.
    /// </summary>
    internal class ExtendedMemberExpression
    {
        public MemberExpression Expression { get; }
        public ITypeMember MemberInfo { get; }

        public ExtendedMemberExpression(Expr expr, ITypeMember memberInfo)
        {
            Expression = Expr.MakeMemberAccess(expr, memberInfo.MemberInfo);
            MemberInfo = memberInfo;
        }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }
}
