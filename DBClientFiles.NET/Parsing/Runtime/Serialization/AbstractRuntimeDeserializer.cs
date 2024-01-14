using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils.Expressions.Extensions;
using System;
using System.Collections.Generic;

using static DBClientFiles.NET.Parsing.Runtime.IExpression;

using System.Linq.Expressions;
using Expr = System.Linq.Expressions.Expression;
using System.Buffers;
using System.Diagnostics;

namespace DBClientFiles.NET.Parsing.Runtime.Serialization
{
    /// <summary>
    /// A simple (I can hear you coughing) base class providing support for generating a 
    /// record type's deserialization method at runtime.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    internal abstract class AbstractRuntimeDeserializer<T>
    {
        protected IExpression Instance { get; set; }

        private IEnumerable<MemberToken> Members { get; }

        private readonly Func<TypeToken, IEnumerable<MemberToken>> MemberProvider;

        private readonly ParameterProvider _iteratorProvider;
        private readonly List<Method.Parameter> Variables = new List<Method.Parameter>();

        public AbstractRuntimeDeserializer(TypeToken typeToken, TypeTokenKind typeTokenType) : base()
        {
            _iteratorProvider = new ParameterProvider(typeof(int));

            MemberProvider = typeTokenType switch
            {
                TypeTokenKind.Field => t => t.Fields,
                TypeTokenKind.Property => t => t.Properties,
                _ => throw new InvalidOperationException(),
            };
            Members = MemberProvider(typeToken);
        }

        protected IExpression CreateBody()
        {
            var methodBlock = new Method.BlockCollection();
            methodBlock.Children.Add(CreateBodyPrologue());

            foreach (var memberToken in Members)
                methodBlock.Children.Add(EmitRead(memberToken, Instance));
            
            methodBlock.Children.Add(CreateBodyEpilogue());

            // Now add all possible extras
            foreach (var requestedParameter in Variables)
                methodBlock.Variables.Add(requestedParameter);

#if DEBUG
            var expr = methodBlock.ToExpression();
            Console.WriteLine(expr.AsString());
#endif
            return methodBlock;
        }

        protected virtual IExpression CreateBodyPrologue() => new Method.Assignment(Instance, Expr.New(typeof(T)).ToMethodBlock());
        protected virtual IExpression CreateBodyEpilogue() => Instance;

        protected abstract UnrollingMode OnLoopGenerationStart(MemberToken memberInfo);
        protected abstract void OnLoopGenerationEnd(MemberToken state);
        protected abstract UnrollingMode OnLoopGenerationIteration(int iterationIndex, MemberToken state);

        /// <summary>
        /// Generates a block expression designed to deserialize a given member.
        /// </summary>
        /// <param name="memberToken">A token describing the member for which code is being generated.</param>
        /// <param name="declaringInstanceAccess">An expression accessing the member for which code is being generated.</param>
        private IExpression EmitRead(MemberToken memberToken, IExpression declaringInstanceAccess)
        {
            var memberAccess = new Method.MemberAccess(declaringInstanceAccess, memberToken);

            return GetEmitKind(memberToken) switch {
                MemberEmitKind.Simple => EmitSimpleMember(memberToken.TypeToken, memberAccess),
                MemberEmitKind.Array  => EmitIterable(memberToken, memberAccess),
                MemberEmitKind.List   => EmitIterable(memberToken, memberAccess),
                _                     => throw new NotImplementedException()
            };
        }

        private enum MemberEmitKind
        {
            Simple,
            Array,
            List
        }

        /// <summary>
        /// Utility method to determine the emission strategy kind for a member.
        /// </summary>
        /// <param name="memberToken"></param>
        /// <returns></returns>
        private static MemberEmitKind GetEmitKind(MemberToken memberToken) {
            if (memberToken.IsArray)
                return MemberEmitKind.Array;

            if (memberToken.TypeToken.Type.IsGenericType) {
                if (memberToken.TypeToken.Type.GetGenericTypeDefinition() == typeof(List<>))
                    return MemberEmitKind.List;
            }

            return MemberEmitKind.Simple;
        }

        /// <summary>
        /// Emits an expression deserializing a field whose type implements IEnumerable<T>.
        /// </summary>
        /// <param name="memberToken">A token describing the member accessed.</param>
        /// <param name="memberAccess">An expression accessing the member described by the token.</param>
        /// <returns>The expression generated.</returns>
        private IExpression EmitIterable(MemberToken memberToken, IExpression memberAccess)
        {
            Debug.Assert(memberToken.TypeToken.Type.GenericTypeArguments.Length == 1);

            // Construct via parameterless constructor.
            var instanceInitializer = memberToken.TypeToken.NewExpression().ToMethodBlock();

            var block = new Method.BlockCollection();
            block.Children.Add(new Method.Assignment(memberAccess, instanceInitializer));

            block.Children.Add(EmitLoop(memberToken, memberAccess, elementAccess => {
                return EmitRead(memberToken, elementAccess);
            }));

            return block;
        }

        /// <summary>
        /// Generates code for deserializing a member.
        /// </summary>
        /// <param name="typeToken"></param>
        /// <param name="memberAccess"></param>
        /// <param name="block"></param>
        private IExpression EmitSimpleMember(TypeToken typeToken, IExpression memberAccess)
        {
            var instanceInitializer = CreateInstanceInitializer(typeToken, memberAccess);
            var defaultInstanceInitializer = instanceInitializer == default(IExpression);
            if (defaultInstanceInitializer && typeToken.IsClass)
                instanceInitializer = new Method.Assignment(memberAccess, typeToken.NewExpression().ToMethodBlock());

            var block = new Method.BlockCollection();

            if (instanceInitializer != default)
                block.Children.Add(instanceInitializer);

            if (!defaultInstanceInitializer)
                return block;

            foreach (var subMemberToken in MemberProvider(typeToken))
                block.Children.Add(EmitRead(subMemberToken, memberAccess));

            return block;
        }

        private void EmitArray(MemberToken memberToken, IExpression memberAccess, Method.BlockCollection block)
        {
            // Get a type token to the element type
            var elementTypeToken = memberToken.TypeToken.GetElementTypeToken();

            // Get array cardinality
            var arraySize = GetCardinality(memberToken);
            var arraySizeExpression = Expr.Constant(arraySize);

            // Implementations of this class may choose to override the way the initializer works.
            // If they do, we don't need to do anything else.
            var arrayInitializer = CreateArrayInitializer(memberToken, memberAccess);
            var isDefaultArrayInitializer = arrayInitializer == default(IExpression) || Method.Empty.Instance.Equals(arrayInitializer);

            // If implementations didn't create an array, do that.
            if (isDefaultArrayInitializer) // Otherwise just assign new T[N];
                arrayInitializer = elementTypeToken.NewArrayBounds(arraySizeExpression).ToMethodBlock();

            if (arrayInitializer != null)
            {
                // Assign the initializer if it exists.
                block.Children.Add(new Method.Assignment(memberAccess, arrayInitializer));
                // If the generator provided a specialized initializer, exit now.
                if (!isDefaultArrayInitializer)
                    return;
            }

            // If loop unrolling is not wanted, reassign arraySize to 1.
            // This is a bit weird, but the basic idea is to avoid duplicating code paths
            // if unrolling is enabled or not.
            var loopUnrollingNeeded = OnLoopGenerationStart(memberToken) != UnrollingMode.Never;
            if (!loopUnrollingNeeded) // Fast path to reduce allocations
                arraySize = 1;

            var elementAccesses = ArrayPool<Method.ArrayAccess>.Shared.Rent(arraySize);
            var loopBodyBlocks = ArrayPool<Method.BlockCollection>.Shared.Rent(arraySize);
            
            for (var i = 0; i < arraySize; ++i)
            {
                elementAccesses[i] = new Method.ArrayAccess(memberAccess, Expr.Constant(i).ToMethodBlock());
                loopBodyBlocks[i] = new Method.BlockCollection();

                // Classes need a public default ctor
                if (elementTypeToken.IsClass && elementTypeToken != typeof(string))
                {
                    var newElementBlock = elementTypeToken.NewExpression().ToMethodBlock();
                    loopBodyBlocks[i].Children.Add(new Method.Assignment(elementAccesses[i], newElementBlock));
                }

                loopBodyBlocks[i].Children.Add(EmitRead(memberToken, elementAccesses[i]));

                // If the implementation decides we never need to unroll this loop, save ourselves the trouble
                var unrollingMode = OnLoopGenerationIteration(i, memberToken);
                if (unrollingMode == UnrollingMode.Never)
                    break;

                // If unrolling state was decided to be unknown, we determine if we needed to unroll
                // This is technically legacy code
                if (unrollingMode == UnrollingMode.Unknown)
                    if (i > 0 && !loopUnrollingNeeded)
                        loopUnrollingNeeded = !loopBodyBlocks[i].Equals(loopBodyBlocks[0]);
            }

            OnLoopGenerationEnd(memberToken);

            // If all the invocation bodies are equal, we can just loop. Otherwise, we need to unroll it.
            if (loopUnrollingNeeded)
            {
                for (var i = 0; i < arraySize; ++i)
                {
                    elementAccesses[i].Index = Expr.Constant(i).ToMethodBlock();
                    foreach (var variable in loopBodyBlocks[i].Variables)
                        block.Variables.Add(variable);

                    block.Children.AddRange(loopBodyBlocks[i].Children);
                }
            }
            else
            {
                // If loop unrolling has not been requested, adjust the element access
                // to refer to the iteration counter

                var iterationCounter = _iteratorProvider.Rent();

                elementAccesses[0].Index = iterationCounter;
                block.Variables.Add(iterationCounter);

                var arraySizeBlock = arraySizeExpression.ToMethodBlock();
                block.Children.Add(new Method.Assignment(iterationCounter, Expr.Constant(0).ToMethodBlock()));
                block.Children.Add(new Method.Loop(iterationCounter, arraySizeBlock, loopBodyBlocks[0]));

                _iteratorProvider.Return(iterationCounter);
            }

            ArrayPool<Method.ArrayAccess>.Shared.Return(elementAccesses);
            ArrayPool<Method.BlockCollection>.Shared.Return(loopBodyBlocks);
        }

        protected abstract IExpression CreateArrayInitializer(MemberToken memberToken, IExpression assignmentTarget);
        protected abstract IExpression CreateInstanceInitializer(TypeToken typeToken, IExpression assignmentTarget);
        protected virtual int GetCardinality(MemberToken memberToken)
            => memberToken.Cardinality;

        protected abstract LoopGenerationState LoopGenerationBehavior();

        /// <summary>
        /// Determines wether deserialization for the provided member should be unrolled. Only called on members
        /// whose types are iterable.
        /// </summary>
        /// <param name="memberToken"></param>
        /// <returns></returns>
        protected abstract bool Unroll(MemberToken memberToken);

        private IExpression EmitLoop(MemberToken memberToken, IExpression memberAccess, Func<IExpression, IExpression> bodyEmitter)
        {
            Debug.Assert(memberToken.IsArray);

            if (Unroll(memberToken)) {
                var cardinality = GetCardinality(memberToken);
                var unrolledIterations = new Method.BlockCollection();

                for (var i = 0; i < cardinality; ++i)
                    unrolledIterations.Children.Add(bodyEmitter(memberToken.MakeArrayAccess(Expr.Constant(i).ToMethodBlock())));

                return unrolledIterations;
            } else {
                var cardinality = Expr.Constant(GetCardinality(memberToken)).ToMethodBlock();
                var iteratorParameter = _iteratorProvider.Rent();

                return new Method.Loop(iteratorParameter, cardinality, bodyEmitter(memberToken.MakeArrayAccess(iteratorParameter)));
            }
        }
    }
}
