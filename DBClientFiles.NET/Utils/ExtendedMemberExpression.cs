using System.Linq.Expressions;

namespace DBClientFiles.NET.Utils
{
    internal struct ExtendedMemberExpression
    {
        public MemberExpression Expression { get; }
        public ExtendedMemberInfo MemberInfo { get; }

        public ExtendedMemberExpression(Expression expr, ExtendedMemberInfo memberInfo)
        {
            Expression = System.Linq.Expressions.Expression.MakeMemberAccess(expr, memberInfo);
            MemberInfo = memberInfo;
        }
    }
}
