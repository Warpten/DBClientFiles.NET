using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils.Expressions.Extensions;
using System;
using System.Collections.Generic;

using static DBClientFiles.NET.Parsing.Runtime.IMethodBlock;

using System.Linq.Expressions;
using Expr = System.Linq.Expressions.Expression;
using System.Buffers;

namespace DBClientFiles.NET.Parsing.Runtime.Serialization
{
    /// <summary>
    /// A simple (I can hear you guys coughing) base class providing support for generating a record type's deserialization method at runtime.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    internal abstract class AbstractRuntimeDeserializer<T>
    {
        protected IMethodBlock Instance { get; set; }

        private IEnumerable<MemberToken> Members { get; }

        private readonly Func<TypeToken, IEnumerable<MemberToken>> MemberProvider;

        private readonly ParameterProvider _iteratorProvider;
        private readonly List<Method.Parameter> Variables = new List<Method.Parameter>();

        public AbstractRuntimeDeserializer(TypeToken typeToken, TypeTokenType typeTokenType) : base()
        {
            _iteratorProvider = new ParameterProvider(typeof(int));

            MemberProvider = typeTokenType switch
            {
                TypeTokenType.Field => t => t.Fields,
                TypeTokenType.Property => t => t.Properties,
                _ => throw new InvalidOperationException(),
            };
            Members = MemberProvider(typeToken);
        }

        protected IMethodBlock CreateBody()
        {
            var methodBlock = new Method.BlockCollection();
            methodBlock.Children.Add(CreateBodyPrologue());

            foreach (var memberToken in Members)
                GenerateMemberBlock(memberToken, Instance, methodBlock);

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

        protected virtual IMethodBlock CreateBodyPrologue() => new Method.Assignment(Instance, Expr.New(typeof(T)).ToMethodBlock());
        protected virtual IMethodBlock CreateBodyEpilogue() => Instance;

        protected abstract UnrollingMode OnLoopGenerationStart(MemberToken memberInfo);
        protected abstract void OnLoopGenerationEnd(MemberToken state);
        protected abstract UnrollingMode OnLoopGenerationIteration(int iterationIndex, MemberToken state);

        protected Method.Parameter AllocateLocalVariable<TVariable>(string variableName)
        {
            var parameter = new Method.Parameter(typeof(TVariable), variableName);
            Variables.Add(parameter);
            return parameter;
        }

        private void GenerateMemberBlock(MemberToken memberToken, IMethodBlock declaringInstanceAccess, Method.BlockCollection block)
        {
            var memberAccess = new Method.MemberAccess(declaringInstanceAccess, memberToken);

            if (memberToken.IsArray)
                GenerateArrayMemberBlock(memberToken, memberAccess, block);
            else
                GenerateValueMemberBlock(memberToken.TypeToken, memberAccess, block);
        }

        private void GenerateValueMemberBlock(TypeToken typeToken, IMethodBlock memberAccess, Method.BlockCollection block)
        {
            var instanceInitializer = CreateInstanceInitializer(typeToken, memberAccess);
            var defaultInstanceInitializer = instanceInitializer == default(IMethodBlock);
            if (defaultInstanceInitializer && typeToken.IsClass)
                instanceInitializer = new Method.Assignment(memberAccess, typeToken.NewExpression().ToMethodBlock());

            if (instanceInitializer != default)
                block.Children.Add(instanceInitializer);

            if (!defaultInstanceInitializer)
                return;

            foreach (var subMemberToken in MemberProvider(typeToken))
                GenerateMemberBlock(subMemberToken, memberAccess, block);
        }

        private void GenerateArrayMemberBlock(MemberToken memberToken, IMethodBlock memberAccess, Method.BlockCollection block)
        {
            var elementTypeToken = memberToken.TypeToken.GetElementTypeToken();

            var arraySize = GetCardinality(memberToken);
            var arraySizeExpression = Expr.Constant(arraySize);

            // Give the generator a chance to create the array
            var arrayInitializer = CreateArrayInitializer(memberToken, memberAccess);
            var defaultArrayInitializer = arrayInitializer == default(IMethodBlock);
            if (defaultArrayInitializer) // Otherwise just assign new T[N];
                arrayInitializer = elementTypeToken.NewArrayBounds(arraySizeExpression).ToMethodBlock();

            if (arrayInitializer != null)
            {
                block.Children.Add(new Method.Assignment(memberAccess, arrayInitializer));
                // If the generator provided a specialized initializer, exit now.
                if (!defaultArrayInitializer)
                    return;
            }

            var loopUnrollingNeeded = OnLoopGenerationStart(memberToken) != UnrollingMode.Never;
            if (!loopUnrollingNeeded) // Fast path to reduce allocations
                arraySize = 1;

            var elementAccesses = ArrayPool<Method.DelegatedArrayAccess>.Shared.Rent(arraySize);
            var loopBodyBlocks = ArrayPool<Method.BlockCollection>.Shared.Rent(arraySize);
            
            for (var i = 0; i < arraySize; ++i)
            {
                elementAccesses[i] = new Method.DelegatedArrayAccess(memberAccess);
                loopBodyBlocks[i] = new Method.BlockCollection();

                // Classes need a public default ctor
                if (elementTypeToken.IsClass && elementTypeToken != typeof(string))
                {
                    var newElementBlock = elementTypeToken.NewExpression().ToMethodBlock();
                    loopBodyBlocks[i].Children.Add(new Method.Assignment(elementAccesses[i], newElementBlock));
                }

                GenerateValueMemberBlock(elementTypeToken, elementAccesses[i], loopBodyBlocks[i]);

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
                var iterationCounter = _iteratorProvider.Rent();

                elementAccesses[0].Index = iterationCounter;
                block.Variables.Add(iterationCounter);

                var arraySizeBlock = arraySizeExpression.ToMethodBlock();
                block.Children.Add(new Method.Assignment(iterationCounter, Expr.Constant(0).ToMethodBlock()));
                block.Children.Add(new Method.Loop(iterationCounter, arraySizeBlock, loopBodyBlocks[0]));

                _iteratorProvider.Return(iterationCounter);
            }

            ArrayPool<Method.DelegatedArrayAccess>.Shared.Return(elementAccesses);
            ArrayPool<Method.BlockCollection>.Shared.Return(loopBodyBlocks);
        }

        protected abstract IMethodBlock CreateArrayInitializer(MemberToken memberToken, IMethodBlock assignmentTarget);
        protected abstract IMethodBlock CreateInstanceInitializer(TypeToken typeToken, IMethodBlock assignmentTarget);
        protected abstract int GetCardinality(MemberToken memberToken);
    }
}
