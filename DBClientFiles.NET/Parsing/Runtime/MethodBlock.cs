using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils.Expressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Expr = System.Linq.Expressions.Expression;

namespace DBClientFiles.NET.Parsing.Runtime
{
    internal interface IMethodBlock : IEquatable<IMethodBlock>
    {
        Expr ToExpression();
    }

    internal static class Method
    {
        private static readonly IMethodBlock[] NoBlocks = new IMethodBlock[0];

        internal class BlockCollection : IMethodBlock, IEquatable<BlockCollection>
        {
            public readonly List<IMethodBlock> Children = new List<IMethodBlock>();
            public readonly HashSet<Parameter> Variables = new HashSet<Parameter>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expr ToExpression()
                => Expr.Block(Variables.Select(v => v.Expression), Children.Select(child => child.ToExpression()));

            public bool Equals(IMethodBlock other)
                => other is BlockCollection collectionOther && Equals(collectionOther);

            public bool Equals(BlockCollection other)
                => Variables.SequenceEqual(other.Variables) && Children.SequenceEqual(other.Children);
        }

        internal class Parameter : IMethodBlock, IEquatable<Parameter>
        {
            internal readonly ParameterExpression Expression;

            public Parameter(Type parameterType)
            {
                Expression = Expr.Parameter(parameterType);
            }
            public Parameter(Type parameterType, string name)
            {
                Expression = Expr.Parameter(parameterType, name);
            }

            public bool Equals(IMethodBlock other)
                => other is Parameter paramOther && Equals(paramOther);

            public bool Equals(Parameter other)
                => other.Expression == Expression;

            public Expr ToExpression() => Expression;
        }

        internal class Assignment : IMethodBlock, IEquatable<Assignment>
        {
            private readonly IMethodBlock Left;
            private readonly IMethodBlock Right;

            public Assignment(IMethodBlock left, IMethodBlock right)
            {
                Left = left;
                Right = right;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expr ToExpression() => Expr.Assign(Left.ToExpression(), Right.ToExpression());

            public bool Equals(IMethodBlock other) 
                => other is Assignment assignmentOther && Equals(assignmentOther);

            public bool Equals(Assignment other) => Left.Equals(other.Left) && Right.Equals(other.Right);
        }

        internal class ArrayAccess : IMethodBlock, IEquatable<ArrayAccess>
        {
            private readonly IMethodBlock Array;
            private readonly IMethodBlock Index;

            public ArrayAccess(IMethodBlock array, IMethodBlock index)
            {
                Array = array;
                Index = index;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expr ToExpression() => Expr.ArrayAccess(Array.ToExpression(), Index.ToExpression());

            public bool Equals(IMethodBlock other)
                => other is ArrayAccess arrayAccessOther && Equals(arrayAccessOther);

            public bool Equals(ArrayAccess other) => Array.Equals(other.Array) && Index.Equals(other.Index);
        }

        internal class ArrayIndex : IMethodBlock, IEquatable<ArrayIndex>
        {
            private readonly IMethodBlock Array;
            private readonly IMethodBlock Index;

            public ArrayIndex(IMethodBlock array, IMethodBlock index)
            {
                Array = array;
                Index = index;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expr ToExpression() => Expr.ArrayIndex(Array.ToExpression(), Index.ToExpression());

            public bool Equals(IMethodBlock other)
                => other is ArrayIndex arrayAccessOther && Equals(arrayAccessOther);

            public bool Equals(ArrayIndex other) => Array.Equals(other.Array) && Index.Equals(other.Index);
        }

        internal struct Convert : IMethodBlock, IEquatable<Convert>
        {
            private readonly IMethodBlock Instance;
            private readonly Type Type;

            public Convert(IMethodBlock instance, Type type)
            {
                Instance = instance;
                Type = type;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expr ToExpression()
                => Expr.Convert(Instance.ToExpression(), Type);

            public bool Equals(IMethodBlock other)
                => other is Convert convertOther && Equals(convertOther);

            public bool Equals(Convert other)
                => Instance.Equals(other.Instance) && Type.Equals(other.Type);
        }

        internal class Property : IMethodBlock, IEquatable<Property>
        {
            private readonly IMethodBlock Instance;
            private readonly PropertyInfo PropertyInfo;
            private readonly IMethodBlock[] Arguments;

            public Property(IMethodBlock instance, PropertyInfo propertyInfo, params IMethodBlock[] arguments)
            {
                Instance = instance;
                PropertyInfo = propertyInfo;
                Arguments = arguments;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expr ToExpression()
                => Expr.Property(Instance.ToExpression(), PropertyInfo, Arguments.Select(a => a.ToExpression()).ToArray());

            public bool Equals(IMethodBlock other)
                => other is Property propertyOther && Equals(propertyOther);

            public bool Equals(Property other)
                => Instance.Equals(other.Instance) && PropertyInfo.Equals(other.PropertyInfo) && Arguments.SequenceEqual(other.Arguments);

        }

        internal class DelegatedArrayAccess : IMethodBlock, IEquatable<DelegatedArrayAccess>
        {
            private readonly IMethodBlock Array;

            public IMethodBlock Index;

            public DelegatedArrayAccess(IMethodBlock array)
            {
                Array = array;
                Index = default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expr ToExpression() => Expr.ArrayAccess(Array.ToExpression(), Index.ToExpression());

            public bool Equals(IMethodBlock other)
                => other is DelegatedArrayAccess delegatedArrayAccessOther && Equals(delegatedArrayAccessOther);

            public bool Equals(DelegatedArrayAccess other) => Array.Equals(other.Array);
        }

        internal class MemberAccess : IMethodBlock, IEquatable<MemberAccess>
        {
            private readonly IMethodBlock DeclaringInstance;
            private readonly MemberToken Member;

            public MemberAccess(IMethodBlock declaringInstance, MemberToken memberToken)
            {
                DeclaringInstance = declaringInstance;
                Member = memberToken;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expr ToExpression() => Expr.MakeMemberAccess(DeclaringInstance.ToExpression(), Member.MemberInfo);

            public bool Equals(IMethodBlock other)
                => other is MemberAccess memberAccessOther && Equals(memberAccessOther);

            public bool Equals(MemberAccess other)
                => DeclaringInstance.Equals(other.DeclaringInstance) && Member.Equals(other.Member);
        }

        internal class Loop : IMethodBlock, IEquatable<Loop>
        {
            private readonly IMethodBlock Iterator;
            private readonly IMethodBlock UpperBound;
            private readonly IMethodBlock Body;

            public Loop(IMethodBlock iterator, IMethodBlock upperBound, IMethodBlock body)
            {
                Iterator = iterator;
                UpperBound = upperBound;
                Body = body;
            }

            public Expr ToExpression()
            {
                var exitLabel = Expr.Label();
                var loopBody = Expr.Block(Body.ToExpression(), Expr.PreIncrementAssign(Iterator.ToExpression()));

                return Expr.Loop(Expr.IfThenElse(Expr.LessThan(Iterator.ToExpression(), UpperBound.ToExpression()), loopBody, Expr.Break(exitLabel)), exitLabel);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(IMethodBlock other)
                => other is Loop loopOther && Equals(loopOther);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(Loop other)
                => Iterator.Equals(other.Iterator) && UpperBound.Equals(other.Iterator) && Body.Equals(other.Body);
        }

        internal class MethodCall : IMethodBlock, IEquatable<MethodCall>
        {
            private readonly IMethodBlock InvocationTarget;
            private readonly MethodInfo Method;
            private readonly IMethodBlock[] Parameters;

            public MethodCall(IMethodBlock invocationTarget, MethodInfo method, params IMethodBlock[] parameters)
            {
                Debug.Assert((invocationTarget == null) == method.IsStatic);

                InvocationTarget = invocationTarget;
                Method = method;
                Parameters = parameters;
            }

            public Expr ToExpression()
            {
                if (Method.IsStatic)
                    return Expr.Call(Method, Parameters.Select(p => p.ToExpression()));

                return Expr.Call(InvocationTarget.ToExpression(), Method, Parameters.Select(p => p.ToExpression()));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(IMethodBlock other) 
                => other is MethodCall methodCallOther && Equals(methodCallOther);

            public bool Equals(MethodCall other)
            {
                if (InvocationTarget != null)
                {
                    if (!InvocationTarget.Equals(other.InvocationTarget))
                        return false;
                }

                if (!Method.Equals(other.Method))
                    return false;

                return Parameters.SequenceEqual(other.Parameters);
            }
        }

        internal class Empty : IMethodBlock, IEquatable<Empty>
        {
            public bool Equals(IMethodBlock other) 
                => other is Empty emptyOther && Equals(emptyOther);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expr ToExpression() => DefaultExpr;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(Empty other) => true;

            private static readonly DefaultExpression DefaultExpr = Expr.Empty();

            public static Empty Instance;
        }

        internal class Expression : IMethodBlock, IEquatable<Expression>
        {
            private readonly Expr _expression;

            public Expression(Expr expression)
            {
                _expression = expression;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expr ToExpression() => _expression;

            public bool Equals(IMethodBlock other)
                => other is Expression exprOther && Equals(exprOther);

            public bool Equals(Expression other)
                => ExpressionEqualityComparer.Instance.Equals(_expression, other._expression);
        }
    }

    internal class Method<T> : IMethodBlock, IEquatable<Method<T>> where T : Delegate
    {
        private readonly IMethodBlock[] Parameters;
        private readonly IMethodBlock Body;

        public Method(IMethodBlock body, params IMethodBlock[] parameters)
        {
            Body = body;
            Parameters = parameters;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LambdaExpression ToLambdaExpression() // TODO: cut OfType
            => Expr.Lambda<T>(Body.ToExpression(), Parameters.Select(p => p.ToExpression()).OfType<ParameterExpression>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Expr ToExpression() => ToLambdaExpression();

        public bool Equals(IMethodBlock other)
            => other is Method<T> exprOther && Equals(exprOther);

        public bool Equals(Method<T> other)
            => Parameters.SequenceEqual(other.Parameters) && Body.Equals(other.Body);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Compile() => (T)ToLambdaExpression().Compile();
    }

    internal static class MethodBlockExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Method.Expression ToMethodBlock(this Expr expr) => new Method.Expression(expr);
    }
}
