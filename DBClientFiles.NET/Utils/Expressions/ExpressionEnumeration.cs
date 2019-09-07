using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Utils.Expressions
{
    internal class ExpressionEnumeration : ExpressionVisitor, IEnumerable<Expression>
    {
        private List<Expression> _expressions = new List<Expression>();

        public ExpressionEnumeration(Expression expression) => Visit(expression);

        public override Expression Visit(Expression expression)
        {
            if (expression == null)
                return null;

            _expressions.Add(expression);
            return base.Visit(expression);
        }

        public IEnumerator<Expression> GetEnumerator() => _expressions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
