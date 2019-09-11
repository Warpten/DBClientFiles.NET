using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Utils.Expressions
{
    internal class ExpressionHashCodeCalculator : ExpressionVisitor
    {
        private int _hashCode;

        private static Lazy<ExpressionHashCodeCalculator> _lazyInstance = new Lazy<ExpressionHashCodeCalculator>(() => new ExpressionHashCodeCalculator());

        public static ExpressionHashCodeCalculator Instance => _lazyInstance.Value;

        private ExpressionHashCodeCalculator() { }

        public int GetHashCode(Expression expression)
        {
            _hashCode = 0;
            Visit(expression);
            return _hashCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(int i) => _hashCode = (_hashCode * 37) ^ i;
        
        public override Expression Visit(Expression expression)
        {
            if (expression == null)
                return null;

            Add((int) expression.NodeType);
            Add(expression.Type.GetHashCode());

            return base.Visit(expression);
        }

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            if (constant != null && constant.Value != null)
                Add(constant.Value.GetHashCode());

            return constant;
        }

        protected override Expression VisitMember(MemberExpression member)
        {
            Add(member.Member.GetHashCode());

            return base.VisitMember(member);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            Add(methodCall.Method.GetHashCode());

            return base.VisitMethodCall(methodCall);
        }

        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            Add(parameter.Name.GetHashCode());

            return parameter;
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression type)
        {
            Add(type.TypeOperand.GetHashCode());

            return base.VisitTypeBinary(type);
        }

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            if (binary.Method != null)
                Add(binary.Method.GetHashCode());
            if (binary.IsLifted)
                Add(1);
            if (binary.IsLiftedToNull)
                Add(1);

            return base.VisitBinary(binary);
        }

        protected override Expression VisitUnary(UnaryExpression unary)
        {
            if (unary.Method != null)
                Add(unary.Method.GetHashCode());
            if (unary.IsLifted)
                Add(1);
            if (unary.IsLiftedToNull)
                Add(1);

            return base.VisitUnary(unary);
        }

        protected override Expression VisitNew(NewExpression nex)
        {
            Add(nex.Constructor.GetHashCode());

            foreach (var member in nex.Members)
                Add(member.MemberType.GetHashCode());

            return base.VisitNew(nex);
        }
    }
}
