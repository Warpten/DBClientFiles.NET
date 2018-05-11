using System.Linq.Expressions;

namespace DBClientFiles.NET.Utils
{
    internal struct ExtendedMemberExpression
    {
        public MemberExpression MemberExpression { get; set; }
        public ExtendedMemberInfo MemberInfo { get; set; }

        public ExtendedMemberExpression(Expression expr, ExtendedMemberInfo memberInfo)
        {
            MemberExpression = Expression.MakeMemberAccess(expr, memberInfo);
            MemberInfo = memberInfo;
        }
    }
}
