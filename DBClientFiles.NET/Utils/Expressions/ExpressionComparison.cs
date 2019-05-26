﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Utils.Expressions
{
    internal class ExpressionComparison : ExpressionVisitor
    {
        private Queue<Expression> _candidates;
        private Expression _candidate;

        public bool AreEqual { get; private set; } = true;

        public ExpressionComparison(Expression a, Expression b)
        {
            _candidates = new Queue<Expression>(new ExpressionEnumeration(b));

            Visit(a);

            if (_candidates.Count > 0) Stop();
        }

        private Expression PeekCandidate()
        {
            if (_candidates.Count == 0) return null;
            return _candidates.Peek();
        }

        private Expression PopCandidate()
        {
            return _candidates.Dequeue();
        }

        private bool CheckAreOfSameType(Expression candidate, Expression expression)
        {
            if (!CheckEqual(expression.NodeType, candidate.NodeType)) return false;
            if (!CheckEqual(expression.Type, candidate.Type)) return false;

            return true;
        }

        private void Stop()
        {
            AreEqual = false;
        }

        private T CandidateFor<T>(T original) where T : Expression
        {
            return (T)_candidate;
        }

        public override Expression Visit(Expression expression)
        {
            if (expression == null) return expression;
            if (!AreEqual) return expression;

            _candidate = PeekCandidate();
            if (!CheckNotNull(_candidate)) return expression;
            if (!CheckAreOfSameType(_candidate, expression)) return expression;

            PopCandidate();

            return base.Visit(expression);
        }

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            var candidate = CandidateFor(constant);
            if (!CheckEqual(constant.Value, candidate.Value)) return null;

            return base.VisitConstant(constant);
        }

        protected override Expression VisitMember(MemberExpression member)
        {
            var candidate = CandidateFor(member);
            if (!CheckEqual(member.Member, candidate.Member)) return null;

            return base.VisitMember(member);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            var candidate = CandidateFor(methodCall);
            if (!CheckEqual(methodCall.Method, candidate.Method)) return null;

            return base.VisitMethodCall(methodCall);
        }

        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            var candidate = CandidateFor(parameter);
            if (!CheckEqual(parameter.Name, candidate.Name)) return null;

            return base.VisitParameter(parameter);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression type)
        {
            var candidate = CandidateFor(type);
            if (!CheckEqual(type.TypeOperand, candidate.TypeOperand)) return null;

            return base.VisitTypeBinary(type);
        }

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            var candidate = CandidateFor(binary);
            if (!CheckEqual(binary.Method, candidate.Method)) return null;
            if (!CheckEqual(binary.IsLifted, candidate.IsLifted)) return null;
            if (!CheckEqual(binary.IsLiftedToNull, candidate.IsLiftedToNull)) return null;

            return base.VisitBinary(binary);
        }

        protected override Expression VisitUnary(UnaryExpression unary)
        {
            var candidate = CandidateFor(unary);
            if (!CheckEqual(unary.Method, candidate.Method)) return null;
            if (!CheckEqual(unary.IsLifted, candidate.IsLifted)) return null;
            if (!CheckEqual(unary.IsLiftedToNull, candidate.IsLiftedToNull)) return null;

            return base.VisitUnary(unary);
        }

        protected override Expression VisitNew(NewExpression nex)
        {
            var candidate = CandidateFor(nex);
            if (!CheckEqual(nex.Constructor, candidate.Constructor)) return null;
            CompareList(nex.Members, candidate.Members);

            return base.VisitNew(nex);
        }

        private void CompareList<T>(ReadOnlyCollection<T> collection, ReadOnlyCollection<T> candidates)
        {
            CompareList(collection, candidates, (item, candidate) => EqualityComparer<T>.Default.Equals(item, candidate));
        }

        private void CompareList<T>(ReadOnlyCollection<T> collection, ReadOnlyCollection<T> candidates, Func<T, T, bool> comparer)
        {
            if (!CheckAreOfSameSize(collection, candidates)) return;

            for (int i = 0; i < collection.Count; i++)
            {
                if (!comparer(collection[i], candidates[i]))
                {
                    Stop();
                    return;
                }
            }
        }

        private bool CheckAreOfSameSize<T>(ReadOnlyCollection<T> collection, ReadOnlyCollection<T> candidate)
        {
            return CheckEqual(collection.Count, candidate.Count);
        }

        private bool CheckNotNull<T>(T t) where T : class
        {
            if (t == null)
            {
                Stop();
                return false;
            }

            return true;
        }

        private bool CheckEqual<T>(T t, T candidate)
        {
            if (!EqualityComparer<T>.Default.Equals(t, candidate))
            {
                Stop();
                return false;
            }

            return true;
        }
    }
}