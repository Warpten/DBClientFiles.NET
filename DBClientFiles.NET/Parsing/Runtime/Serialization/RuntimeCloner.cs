using DBClientFiles.NET.Parsing.Reflection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace DBClientFiles.NET.Parsing.Runtime.Serialization
{
    /// <summary>
    /// A simple (i can *still* hear you guys coughing) class providing support for cloning any record type.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    internal readonly struct RuntimeCloner<T>
    {
        private delegate void CloningMethod(in T source, out T target);
        private readonly CloningMethod _cloneMethod;
        private readonly ParameterProvider _iteratorProvider;

        public RuntimeCloner(TypeToken typeToken, TypeTokenKind tokenType)
        {
            _iteratorProvider = new ParameterProvider(typeof(int));
            _cloneMethod = null; // Keep compiler happy

            var oldInstanceParam = Expression.Parameter(typeof(T).MakeByRefType(), "source");
            var newInstanceParam = Expression.Parameter(typeof(T).MakeByRefType(), "target");

            var body = new List<Expression> {
                Expression.Assign(newInstanceParam, Expression.New(typeof(T)))
            };

            foreach (var memberInfo in typeToken.Members)
            {
                var oldMemberAccess = memberInfo.MakeAccess(oldInstanceParam);
                var newMemberAccess = memberInfo.MakeAccess(newInstanceParam);

                body.Add(CloneMember(memberInfo.TypeToken, oldMemberAccess, newMemberAccess, tokenType));
            }

            body.Add(newInstanceParam);

            var bodyBlock = Expression.Block(body);
            var lambda = Expression.Lambda<CloningMethod>(bodyBlock, oldInstanceParam, newInstanceParam);
            _cloneMethod = lambda.Compile();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clone(in T source, out T target) => _cloneMethod.Invoke(in source, out target);

        private Expression CloneMember(TypeToken memberToken, Expression oldMember, Expression newMember, TypeTokenKind tokenType)
        {
            if (oldMember.Type.IsArray)
            {
                var loopItrBlk = _iteratorProvider.Rent();
                var loopItr = loopItrBlk._expression;

                var lengthValue = Expression.MakeMemberAccess(oldMember,
                    oldMember.Type.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance));
                var newArrayExpr = memberToken.GetElementTypeToken().NewArrayBounds(lengthValue);

                var loopCondition = Expression.LessThan(loopItr, lengthValue);
                var loopExitLabel = Expression.Label();

                var loopBodyBlock = CloneMember(memberToken.GetElementTypeToken(),
                    Expression.ArrayAccess(oldMember, loopItr),
                    Expression.ArrayAccess(newMember, loopItr),
                    tokenType);

                var loopBody = Expression.Block(new[] { loopItr },
                    Expression.Assign(newMember, newArrayExpr),
                    Expression.Assign(loopItr, Expression.Constant(0)),
                    Expression.Loop(
                        Expression.IfThenElse(loopCondition,
                            Expression.Block(
                                loopBodyBlock,
                                Expression.PreIncrementAssign(loopItr)
                            ),
                            Expression.Break(loopExitLabel)
                        ), loopExitLabel));

                _iteratorProvider.Return(loopItrBlk);
                return loopBody;
            }

            if (memberToken == typeof(string) || memberToken.IsPrimitive)
                return Expression.Assign(newMember, oldMember);

            var block = new List<Expression>() {
                Expression.Assign(newMember, Expression.New(newMember.Type))
            };

            foreach (var childInfo in memberToken.Members)
            {
                if (tokenType != childInfo.MemberType || childInfo.IsReadOnly)
                    continue;

                var oldChild = childInfo.MakeAccess(oldMember);
                var newChild = childInfo.MakeAccess(newMember);

                var childBodyBlock = CloneMember(childInfo.TypeToken, oldChild, newChild, tokenType);
                block.Add(childBodyBlock);
            }

            if (block.Count == 1)
                return block[0];
            return Expression.Block(block);
        }
    }
}
