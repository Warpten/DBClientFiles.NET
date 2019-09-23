using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

using System.Linq.Expressions;
using System.Threading;
using Expr = System.Linq.Expressions.Expression;

namespace DBClientFiles.NET.Utils.Expressions
{
    internal class ExpressionEqualityComparer : ExpressionVisitor, IEqualityComparer<Expr>
    {
        private IEnumerator<Expr> _candidates;
        private Expr _candidate;

        private static readonly ThreadLocal<ExpressionEqualityComparer> _instance =
            new ThreadLocal<ExpressionEqualityComparer>(() => new ExpressionEqualityComparer());

        public static ExpressionEqualityComparer Instance { get; } = _instance.Value;

        public int GetHashCode(Expr obj)
            => ExpressionHashCodeCalculator.Instance.GetHashCode(obj);

        public bool Equals(Expr left, Expr right)
        {
            if (left == right)
                return true;

            _candidates = new ExpressionEnumeration(right).GetEnumerator();
            if (!_candidates.MoveNext())
                return false;

            Visit(left);
            return !_candidates.MoveNext();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Expr PeekCandidate() => _candidates.Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PopCandidate() => _candidates.MoveNext();

        private bool CheckAreOfSameType(Expr candidate, Expr expression)
        {
            if (!CheckEqual(expression.NodeType, candidate.NodeType))
                return false;
            if (!CheckEqual(expression.Type, candidate.Type))
                return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T CandidateFor<T>() where T : Expr => (T)_candidate;

        public override Expr Visit(Expr expression)
        {
            if (expression == null)
                return null;

            _candidate = PeekCandidate();

            // If candidate is null, return
            if (!CheckNotNull(_candidate))
                return expression;

            // If candidate expression type mismatches, return
            if (!CheckAreOfSameType(_candidate, expression))
                return expression;

            PopCandidate();

            return base.Visit(expression);
        }

        protected override Expr VisitConstant(ConstantExpression constant)
        {
            var candidate = CandidateFor<ConstantExpression>();
            if (!CheckEqual(constant.Value, candidate.Value))
                return null;

            return base.VisitConstant(constant);
        }

        protected override Expr VisitMember(MemberExpression member)
        {
            var candidate = CandidateFor<MemberExpression>();
            if (!CheckEqual(member.Member, candidate.Member))
                return null;

            return base.VisitMember(member);
        }

        protected override Expr VisitMethodCall(MethodCallExpression methodCall)
        {
            var candidate = CandidateFor<MethodCallExpression>();
            if (!CheckEqual(methodCall.Method, candidate.Method))
                return null;

            return base.VisitMethodCall(methodCall);
        }

        protected override Expr VisitParameter(ParameterExpression parameter)
        {
            var candidate = CandidateFor<ParameterExpression>();
            if (!CheckEqual(parameter.Name, candidate.Name))
                return null;

            return base.VisitParameter(parameter);
        }

        protected override Expr VisitTypeBinary(TypeBinaryExpression type)
        {
            var candidate = CandidateFor<TypeBinaryExpression>();
            if (!CheckEqual(type.TypeOperand, candidate.TypeOperand))
                return null;

            return base.VisitTypeBinary(type);
        }

        protected override Expr VisitBinary(BinaryExpression binary)
        {
            var candidate = CandidateFor<BinaryExpression>();
            if (!CheckEqual(binary.Method, candidate.Method))
                return null;
            if (!CheckEqual(binary.IsLifted, candidate.IsLifted))
                return null;
            if (!CheckEqual(binary.IsLiftedToNull, candidate.IsLiftedToNull))
                return null;

            return base.VisitBinary(binary);
        }

        protected override Expr VisitUnary(UnaryExpression unary)
        {
            var candidate = CandidateFor<UnaryExpression>();
            if (!CheckEqual(unary.Method, candidate.Method))
                return null;
            if (!CheckEqual(unary.IsLifted, candidate.IsLifted))
                return null;
            if (!CheckEqual(unary.IsLiftedToNull, candidate.IsLiftedToNull))
                return null;

            return base.VisitUnary(unary);
        }

        protected override Expr VisitNew(NewExpression nex)
        {
            var candidate = CandidateFor<NewExpression>();
            if (!CheckEqual(nex.Constructor, candidate.Constructor))
                return null;

            if (!CompareList(nex.Members, candidate.Members))
                return null;

            return base.VisitNew(nex);
        }

        protected override Expr VisitSwitch(SwitchExpression se)
        {
            var candidate = CandidateFor<SwitchExpression>();
            if (!CheckEqual(se.DefaultBody, candidate.DefaultBody))
                return null;

            if (!CheckEqual(se.SwitchValue, candidate.SwitchValue))
                return null;

            if (!CheckEqual(se.Comparison, candidate.Comparison))
                return null;

            if (!CompareList(se.Cases, candidate.Cases))
                return null;

            return base.VisitSwitch(se);
        }

        private bool CompareList<T>(ReadOnlyCollection<T> collection, ReadOnlyCollection<T> candidates)
        {
            if (!CheckEqual(collection.Count, candidates.Count))
                return false;

            for (var i = 0; i < collection.Count; i++)
                if (!EqualityComparer<T>.Default.Equals(collection[i], candidates[i]))
                    return false;

            return true;
        }

        private bool CheckNotNull<T>(T t) where T : class
        {
            if (t == null)
                return false;

            return true;
        }

        private bool CheckEqual<T>(T t, T candidate)
        {
            if (!EqualityComparer<T>.Default.Equals(t, candidate))
                return false;

            return true;
        }
    }
}
