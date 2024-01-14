using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Expr = System.Linq.Expressions.Expression;

namespace DBClientFiles.NET.Parsing.Runtime
{
    public interface IExpression {
        Expr ToExpression();
    }

    /// <summary>
    /// A wrapper around <see cref="Expr">Expression Tree expressions</see>.
    /// </summary>
    public interface IExpression<T> : IExpression where T : IExpression<T> {
    }

    internal static class Method
    {
        /// <summary>
        /// Describes a collection of execution blocks in a method.
        /// </summary>
        internal class BlockCollection : IExpression<BlockCollection>
        {
            public readonly List<IExpression> Children = new();
            public readonly HashSet<Parameter> Variables = new();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expr ToExpression()
                => Expr.Block(Variables.Select(v => v.Expression), Children.Select(child => child.ToExpression()));
        }

        /// <summary>
        /// Describes a parameter to a method.
        /// </summary>
        internal class Parameter : IExpression<Parameter>
        {
            public readonly ParameterExpression Expression;

            /// <summary>
            /// Creates a new parameter of the given type.
            /// </summary>
            /// <param name="parameterType">The type of the parameter.</param>
            public Parameter(Type parameterType) {
                Expression = Expr.Parameter(parameterType);
            }

            /// <summary>
            /// Creates a new parameter with the given type and name.
            /// </summary>
            /// <param name="parameterType">The type of the parameter.</param>
            /// <param name="name">The name of the parameter.</param>
            public Parameter(Type parameterType, string name) {
                Expression = Expr.Parameter(parameterType, name);
            }

            public Expr ToExpression() => Expression;
        }

        public static Parameter NewParameter(Type parameterType, string parameterName) => new(parameterType, parameterName);
        public static Parameter NewParameter<T>(string parameterName) => NewParameter(typeof(T), parameterName);
        public static Parameter NewParameter(Type parameterType) => new(parameterType);
        public static Parameter NewParameter<T>() => NewParameter(typeof(T));

        internal class Assignment : IExpression<Assignment>
        {
            private readonly IExpression _left;
            private readonly IExpression _right;

            /// <summary>
            /// Creates a new expression of the following form:
            /// <code>left = right</code>
            /// </summary>
            /// <param name="left">The left expression</param>
            /// <param name="right">The right expression</param>
            public Assignment(IExpression left, IExpression right)
            {
                _left = left;
                _right = right;
            }

            public Expr ToExpression() => Expr.Assign(_left.ToExpression(), _right.ToExpression());
        }

        /// <summary>
        /// Describes an array subscript expression (A[I]).
        /// </summary>
        internal class ArrayAccess : IExpression<ArrayAccess>
        {
            private readonly IExpression Array;
            public IExpression? Index;

            /// <summary>
            /// Creates a new expression of the following form:
            /// <code>array[index]</code>
            /// </summary>
            /// <param name="array">An expression corresponding to the array being indexed.</param>
            /// <param name="index">An expression corresponding to the index used.</param>
            public ArrayAccess(IExpression array, IExpression? index = default)
            {
                Array = array;
                Index = index;
            }

            public Expr ToExpression()
                => Expr.ArrayAccess(Array.ToExpression(), Index.ToExpression());
        }

        internal class ArrayIndex : IExpression<ArrayIndex>
        {
            private readonly IExpression Array;
            private readonly IExpression Index;

            public ArrayIndex(IExpression array, IExpression index)
            {
                Array = array;
                Index = index;
            }

            public Expr ToExpression() => Expr.ArrayIndex(Array.ToExpression(), Index.ToExpression());
        }

        internal class Convert : IExpression<Convert>
        {
            private readonly IExpression Instance;
            private readonly Type Type;

            /// <summary>
            /// Constructs a new expression of the following form:
            /// <code>(type) instance</code>
            /// </summary>
            /// <param name="instance">An expression being converted</param>
            /// <param name="type">The type to which the given <see cref="instance"/> is being cast to.</param>
            public Convert(IExpression instance, Type type)
            {
                Instance = instance;
                Type = type;
            }

            public Expr ToExpression()
                => Expr.Convert(Instance.ToExpression(), Type);
        }

        internal class Property : IExpression<Property>
        {
            private readonly IExpression Instance;
            private readonly PropertyInfo PropertyInfo;
            private readonly IExpression[] Arguments;

            public Property(IExpression instance, PropertyInfo propertyInfo, params IExpression[] arguments)
            {
                Instance = instance;
                PropertyInfo = propertyInfo;
                Arguments = arguments;
            }

            public Expr ToExpression()
                => Expr.Property(Instance.ToExpression(), PropertyInfo, Arguments.Select(a => a.ToExpression()).ToArray());

        }

        /// <summary>
        /// Describes a member access expression (A.B).
        /// </summary>
        internal class MemberAccess : IExpression<MemberAccess>
        {
            private readonly IExpression DeclaringInstance;
            private readonly MemberToken Member;

            public MemberAccess(IExpression declaringInstance, MemberToken memberToken)
            {
                DeclaringInstance = declaringInstance;
                Member = memberToken;
            }

            public Expr ToExpression() => Expr.MakeMemberAccess(DeclaringInstance.ToExpression(), Member.MemberInfo);
        }

        /// <summary>
        /// Describes a loop expression of the following form.
        /// </summary>
        /// <example>
        /// while (Iterator < UpperBound) {
        ///     Body;
        ///     ++Iterator;
        /// }</example>
        internal class Loop : IExpression<Loop>
        {
            private readonly IExpression Iterator;
            private readonly IExpression UpperBound;
            private readonly IExpression Body;

            public Loop(IExpression iterator, IExpression upperBound, IExpression body)
            {
                Iterator = iterator;
                UpperBound = upperBound;
                Body = body;
            }

            public Expr ToExpression()
            {
                var exitLabel = Expr.Label();
                var loopBody = Expr.Block(Body.ToExpression(), Expr.PreIncrementAssign(Iterator.ToExpression()));

                return Expr.Loop(
                    Expr.IfThenElse(
                        Expr.LessThan(Iterator.ToExpression(), UpperBound.ToExpression()),
                        loopBody,
                        Expr.Break(exitLabel)),
                    exitLabel);
            }
        }

        /// <summary>
        /// Describes a method call expression of the following forms:
        /// <ul>
        ///     <li><code>InvocationTarget.Method(Parameters...)</code> in the case of a member method.</li>
        ///     <li><code>typeof(InvocationTarget).Method(Parameters...)</code> in the case of a static method.</li>
        /// </ul>
        /// </summary>
        internal class MethodCall : IExpression<MethodCall>
        {
            private readonly IExpression _invocationTarget;
            private readonly MethodInfo _method;
            private readonly IExpression[] _parameters;

            /// <summary>
            /// Creates a new instance of a method call expression on a static method with the given parameters.
            /// The generated expression will be <code>method(parameters...)</code>
            /// </summary>
            /// <param name="method">The method being called.</param>
            /// <param name="parameters">Parameters to the method call.</param>
            public MethodCall(MethodInfo method, params IExpression[] parameters)
            {
                Debug.Assert(method.IsStatic);

                _invocationTarget = default;
                _method = method;
                _parameters = parameters;
            }

            /// <summary>
            /// Creates a new instance of a method call expression on a member method with the given parameters.
            /// The generated expression will be <code>invocationTarget.method(parameters...)</code>
            /// </summary>
            /// <param name="invocationTarget">The object on which a method is being called.</param>
            /// <param name="method">The method being called.</param>
            /// <param name="parameters">Parameters to the method call.</param>
            public MethodCall(IExpression invocationTarget, MethodInfo method, params IExpression[] parameters)
            {
                // TODO: Assert that the type of the object pointed at by invocationTarget matches method.DeclaringType.

                Debug.Assert(invocationTarget != default && !method.IsStatic);

                _invocationTarget = invocationTarget;
                _method = method;
                _parameters = parameters;
            }

            public Expr ToExpression()
            {
                if (_method.IsStatic)
                    return Expr.Call(_method, _parameters.Select(p => p.ToExpression()));

                return Expr.Call(_invocationTarget.ToExpression(), _method, _parameters.Select(p => p.ToExpression()));
            }
        }

        /// <summary>
        /// An empty expression. This does effectively nothing.
        /// </summary>
        internal struct Empty : IExpression<Empty>
        {
            public bool Equals(IExpression other) 
                => other is Empty emptyOther && Equals(emptyOther);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expr ToExpression() => DefaultExpr;

            private static readonly DefaultExpression DefaultExpr = Expr.Empty();
            public static Empty Instance;
        }

        internal class Expression : IExpression<Expression>
        {
            private readonly Expr _expression;

            public Expression(Expr expression)
            {
                _expression = expression;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expr ToExpression() => _expression;
        }
    }

    internal class Method<T> : IExpression<Method<T>> where T : Delegate
    {
        private readonly IExpression[] Parameters;
        private readonly IExpression Body;

        public Method(IExpression body, params IExpression[] parameters)
        {
            Body = body;
            Parameters = parameters;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LambdaExpression ToLambdaExpression() // TODO: cut OfType
            => Expr.Lambda<T>(Body.ToExpression(), Parameters.Select(p => p.ToExpression()).OfType<ParameterExpression>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Expr ToExpression() => ToLambdaExpression();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Compile() => (T)ToLambdaExpression().Compile();
    }

    internal static class MethodBlockExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Method.Expression ToMethodBlock(this Expr expr) => new(expr);
    }
}
