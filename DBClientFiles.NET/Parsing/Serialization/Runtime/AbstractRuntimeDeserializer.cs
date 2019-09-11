using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils.Expressions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Parsing.Serialization.Runtime
{
    internal abstract class AbstratctRuntimeDeserializer<T>
    {
        protected virtual ParameterExpression Instance { get; }

        private IEnumerable<MemberToken> Members { get; }

        private readonly Func<TypeToken, IEnumerable<MemberToken>> MemberProvider;

        public AbstratctRuntimeDeserializer(TypeToken typeToken, TypeTokenType typeTokenType) : base()
        {
            switch (typeTokenType)
            {
                case TypeTokenType.Field:
                    MemberProvider = t => t.Fields;
                    break;
                case TypeTokenType.Property:
                    MemberProvider = t => t.Properties;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            Members = MemberProvider(typeToken);
        }

        protected Expression CreateBody()
        {
            var instanceBlock = Instance.ToMethodBlock();

            var methodBlock = new MethodBlock.Collection();
            methodBlock.Children.Add(new MethodBlock.Assignment(instanceBlock, Expression.New(typeof(T)).ToMethodBlock()));

            foreach (var memberToken in Members)
                GenerateMemberBlock(memberToken, instanceBlock, methodBlock);

            methodBlock.Children.Add(instanceBlock);

            var expr = methodBlock.ToExpression();
#if DEBUG
            Console.WriteLine(expr.AsString());
#endif
            ParameterProvider<int>.Instance.Clear();

            return expr;
        }

        private void GenerateMemberBlock(MemberToken memberToken, MethodBlock declaringInstanceAccess, MethodBlock.Collection block)
        {
            var memberAccess = new MethodBlock.MemberAccess(declaringInstanceAccess, memberToken);

            if (memberToken.IsArray)
                GenerateArrayMemberBlock(memberToken, memberAccess, block);
            else
                GenerateValueMemberBlock(memberToken.TypeToken, memberAccess, block);
        }

        private void GenerateValueMemberBlock(TypeToken typeToken, MethodBlock memberAccess, MethodBlock.Collection block)
        {
            var instanceInitializer = CreateInstanceInitializer(typeToken)?.ToMethodBlock();
            var defaultInstanceInitializer = instanceInitializer == default;
            if (defaultInstanceInitializer)
            {
                if (typeToken.IsClass)
                    instanceInitializer = typeToken.NewExpression().ToMethodBlock();
            }

            if (instanceInitializer != default)
                block.Children.Add(new MethodBlock.Assignment(memberAccess, instanceInitializer));

            if (!defaultInstanceInitializer)
                return;

            foreach (var subMemberToken in MemberProvider(typeToken))
                GenerateMemberBlock(subMemberToken, memberAccess, block);
        }

        private void GenerateArrayMemberBlock(MemberToken memberToken, MethodBlock memberAccess, MethodBlock.Collection block)
        {
            var elementTypeToken = memberToken.TypeToken.GetElementTypeToken();

            var arraySize = GetCardinality(memberToken);
            var arraySizeExpression = Expression.Constant(arraySize);

            // Give the generator a chance to create the array
            var arrayInitializer = CreateArrayInitializer(memberToken)?.ToMethodBlock();
            var defaultArrayInitializer = arrayInitializer == default;
            if (defaultArrayInitializer) // Otherwise just assign new T[N];
                arrayInitializer = elementTypeToken.NewArrayBounds(arraySizeExpression).ToMethodBlock();

            block.Children.Add(new MethodBlock.Assignment(memberAccess, arrayInitializer));
            // If the generator provided a specialized initializer, exit now.
            if (!defaultArrayInitializer)
                return;


            var elementAccesses = new MethodBlock.DelegatedArrayAccess[arraySize];
            var loopBodyBlocks = new MethodBlock.Collection[arraySize];
            var unrollingNeeded = false;
            for (var i = 0; i < arraySize; ++i)
            {
                elementAccesses[i] = new MethodBlock.DelegatedArrayAccess(memberAccess);
                loopBodyBlocks[i] = new MethodBlock.Collection();

                // Classes need a public default ctor
                if (elementTypeToken.IsClass && elementTypeToken != typeof(string))
                {
                    var newElementBlock = elementTypeToken.NewExpression().ToMethodBlock();
                    loopBodyBlocks[i].Children.Add(new MethodBlock.Assignment(elementAccesses[i], newElementBlock));
                }

                GenerateValueMemberBlock(elementTypeToken, elementAccesses[i], loopBodyBlocks[i]);

                if (i > 0 && !unrollingNeeded)
                    unrollingNeeded = !loopBodyBlocks[i].Equals(loopBodyBlocks[0]);
            }

            // If all the invocation bodies are equal, we can just loop. Otherwise, we need to unroll it.
            if (unrollingNeeded)
            {
                for (var i = 0; i < arraySize; ++i)
                {
                    elementAccesses[i].Index = Expression.Constant(i);
                    foreach (var variable in loopBodyBlocks[i].Variables)
                        block.Variables.Add(variable);

                    block.Children.AddRange(loopBodyBlocks[i].Children);
                }
            }
            else
            {
                var iterationCounter = ParameterProvider<int>.Instance.Rent();
                var iterationCounterBlock = iterationCounter.ToMethodBlock();

                elementAccesses[0].Index = iterationCounter;
                block.Variables.Add(iterationCounter);

                var arraySizeBlock = arraySizeExpression.ToMethodBlock();
                block.Children.Add(new MethodBlock.Assignment(iterationCounterBlock, Expression.Constant(0).ToMethodBlock()));
                block.Children.Add(new MethodBlock.Loop(iterationCounterBlock, arraySizeBlock, loopBodyBlocks[0]));

                ParameterProvider<int>.Instance.Return(iterationCounter);
            }
        }

        protected abstract Expression CreateArrayInitializer(MemberToken memberToken);
        protected abstract Expression CreateInstanceInitializer(TypeToken typeToken);
        protected abstract int GetCardinality(MemberToken memberToken);
    }
}
