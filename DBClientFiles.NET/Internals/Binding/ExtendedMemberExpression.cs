using System.Linq.Expressions;

namespace DBClientFiles.NET.Internals.Binding
{
    /// <summary>
    /// A simple wrapper around <see cref="MemberExpression"/> and <see cref="ExtendedMemberInfo"/>.
    /// </summary>
    internal readonly struct ExtendedMemberExpression
    {
        public MemberExpression Expression { get; }
        public ExtendedMemberInfo MemberInfo { get; }

        public ExtendedMemberExpression(Expression expr, ExtendedMemberInfo memberInfo)
        {
            Expression = System.Linq.Expressions.Expression.MakeMemberAccess(expr, memberInfo.MemberInfo);
            MemberInfo = memberInfo;
        }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }
}
