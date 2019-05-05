using System.Collections.Generic;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Utils.Expressions
{
    public class ExpressionEqualityComparer : IEqualityComparer<Expression>
    {
        public static ExpressionEqualityComparer Instance = new ExpressionEqualityComparer();

        public bool Equals(Expression a, Expression b)
        {
            return new ExpressionComparison(a, b).AreEqual;
        }

        public int GetHashCode(Expression expression)
        {
            return new HashCodeCalculator(expression).HashCode;
        }
    }
}
