using DBClientFiles.NET.Parsing.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Expr = System.Linq.Expressions.Expression;
using System.Text;
using System.Linq.Expressions;
using DBClientFiles.NET.Utils.Expressions;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Parsing.Serialization.Runtime
{
    internal abstract class MethodBlock : IEquatable<MethodBlock>
    {
        public abstract Expr ToExpression();
        public abstract bool Equals(MethodBlock other);

        internal class Collection : MethodBlock, IEquatable<Collection>
        {
            public List<MethodBlock> Children { get; } = new List<MethodBlock>();
            public HashSet<ParameterExpression> Variables { get; } = new HashSet<ParameterExpression>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override Expr ToExpression()
                => Expr.Block(Variables, Children.Select(child => child.ToExpression()));
            public override bool Equals(MethodBlock other)
                => other is Collection collectionOther ? Equals(collectionOther) : false;

            public bool Equals(Collection other)
                => Variables.SequenceEqual(other.Variables) && Children.SequenceEqual(other.Children);
        }

        internal class Assignment : MethodBlock, IEquatable<Assignment>
        {
            public MethodBlock Left { get; }
            public MethodBlock Right { get; }

            public Assignment(MethodBlock left, MethodBlock right) : base()
            {
                Left = left;
                Right = right;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override Expr ToExpression() => Expr.Assign(Left.ToExpression(), Right.ToExpression());

            public override bool Equals(MethodBlock other) 
                => other is Assignment assignmentOther ? Equals(assignmentOther) : false;

            public bool Equals(Assignment other) => Left.Equals(other.Left) && Right.Equals(other.Right);
        }

        internal class DelegatedArrayAccess : MethodBlock, IEquatable<DelegatedArrayAccess>
        {
            public MethodBlock Array { get; }
            public Expr Index { get; set; }

            public DelegatedArrayAccess(MethodBlock array) : base()
            {
                Array = array;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override Expr ToExpression() => Expr.ArrayAccess(Array.ToExpression(), Index);

            public override bool Equals(MethodBlock other)
                => other is DelegatedArrayAccess delegatedArrayAccessOther ? Equals(delegatedArrayAccessOther) : false;

            public bool Equals(DelegatedArrayAccess other) => Array.Equals(other.Array);
        }

        internal class MemberAccess : MethodBlock, IEquatable<MemberAccess>
        {
            public MethodBlock DeclaringInstance { get; }
            public MemberToken Member { get; }

            public MemberAccess(MethodBlock declaringInstance, MemberToken memberToken) : base()
            {
                DeclaringInstance = declaringInstance;
                Member = memberToken;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override Expr ToExpression() => Expr.MakeMemberAccess(DeclaringInstance.ToExpression(), Member.MemberInfo);

            public override bool Equals(MethodBlock other)
                => other is MemberAccess memberAccessOther ? Equals(memberAccessOther) : false;

            public bool Equals(MemberAccess other)
                => DeclaringInstance.Equals(other.DeclaringInstance) && Member.Equals(other.Member);
        }

        internal class Expression : MethodBlock, IEquatable<Expression>
        {
            private readonly Expr _expression;

            public Expression(Expr expression) : base()
            {
                _expression = expression;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override Expr ToExpression() => _expression;

            public override bool Equals(MethodBlock other)
                => other is Expression exprOther ? Equals(exprOther) : false;

            public bool Equals(Expression other) => ExpressionEqualityComparer.Instance.Equals(_expression, other._expression);
        }

        internal class Loop : MethodBlock, IEquatable<Loop>
        {
            private Expression Iterator { get; }
            private Expression UpperBound { get; }
            private MethodBlock Body { get; }

            public Loop(Expression iterator, Expression upperBound, MethodBlock body) : base()
            {
                Iterator = iterator;
                UpperBound = upperBound;
                Body = body;
            }

            public override Expr ToExpression()
            {
                var exitLabel = Expr.Label();
                var loopBody = Expr.Block(Body.ToExpression(), Expr.PreIncrementAssign(Iterator.ToExpression()));

                return Expr.Loop(Expr.IfThenElse(Expr.LessThan(Iterator.ToExpression(), UpperBound.ToExpression()), loopBody, Expr.Break(exitLabel)), exitLabel);
            }

            public override bool Equals(MethodBlock other)
                => other is Loop loopOther ? Equals(loopOther) : false;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(Loop other)
                => Iterator.Equals(other.Iterator) && UpperBound.Equals(other.Iterator) && Body.Equals(other.Body);
        }
    }

    internal static class MethodBlockExtensions
    {
        public static MethodBlock.Expression ToMethodBlock(this Expr expr) => new MethodBlock.Expression(expr);
    }
}
