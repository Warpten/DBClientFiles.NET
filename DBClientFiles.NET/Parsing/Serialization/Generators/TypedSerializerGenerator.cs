using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Parsing.Reflection;

using System.Linq.Expressions;
using Expr = System.Linq.Expressions.Expression;

namespace DBClientFiles.NET.Parsing.Serialization.Generators
{
    /// <summary>
    /// The base class in charge of generating deserialization methods for a given <see cref="T"/>.
    /// </summary>
    /// <typeparam name="T">The record type for which a deserializer must be generated.</typeparam>
    /// <typeparam name="TMethod"></typeparam>
    internal abstract class TypedSerializerGenerator<T, TMethod> : SerializerGenerator where TMethod : Delegate
    {
        protected TypedSerializerGenerator(TypeToken root, TypeTokenType memberType) : base(root, memberType)
        {
            Debug.Assert(root == typeof(T));
        }

        private TMethod GenerateDeserializer()
        {
            var body = GenerateDeserializationMethodBody();

            return MakeLambda(body).Compile();
        }

        private TMethod _methodImpl;

        public TMethod Method
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _methodImpl ??= GenerateDeserializer();
        }

        protected abstract Expression<TMethod> MakeLambda(Expression body);

        protected override TreeNode MakeRootNode()
            => new () {
                AccessExpression = ProducedInstance,
                MemberToken = null,
                TypeToken = Root
            };

        protected sealed override Expr MakeRootMemberAccess(MemberToken token) => token.MakeAccess(ProducedInstance);
        
        protected sealed override Expr MakeReturnExpression() => ProducedInstance;
    
        protected abstract ParameterExpression ProducedInstance { get; }
    }

    /// <summary>
    /// The base class in charge of generating deserialization methods for a given <see cref="{T}"/>.
    /// </summary>
    /// <typeparam name="T">The record type for which a deserializer must be generated.</typeparam>
    /// <typeparam name="TMethod"></typeparam>
    /// <typeparam name="TGenerationState">A state object that is used when generating reader calls.</typeparam>
    internal abstract class TypedSerializerGenerator<T, TMethod, TGenerationState> : TypedSerializerGenerator<T, TMethod> where TMethod : Delegate
    {
        protected TGenerationState State { get; set; }

        protected TypedSerializerGenerator(TypeToken root, TypeTokenType memberType, TGenerationState state) : base(root, memberType)
        {
            Debug.Assert(root == typeof(T));

            State = state;
        }
    }
}
