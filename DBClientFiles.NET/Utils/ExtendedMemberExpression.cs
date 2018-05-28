using System.Linq.Expressions;

namespace DBClientFiles.NET.Utils
{
    /// <summary>
    /// A simple wrapper around <see cref="MemberExpression"/> and <see cref="ExtendedMemberInfo"/>.
    /// </summary>
    internal struct ExtendedMemberExpression
    {
        public MemberExpression Expression;
        public ExtendedMemberInfo MemberInfo;

        public ExtendedMemberExpression(Expression expr, ref ExtendedMemberInfo memberInfo)
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
