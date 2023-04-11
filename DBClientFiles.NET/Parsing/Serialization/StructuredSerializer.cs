using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Versions;
using TypeToken = DBClientFiles.NET.Parsing.Reflection.TypeToken;

using System.Linq.Expressions;
using Expr = System.Linq.Expressions.Expression;

namespace DBClientFiles.NET.Parsing.Serialization
{
    /// <summary>
    /// An abstract implementation of a type serializer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class StructuredSerializer<T>
    {
        protected delegate void TypeCloner(in T source, out T target);
        protected delegate int TypeKeyGetter(in T source);
        protected delegate void TypeKeySetter(out T source, int key);

        private TypeCloner _cloneMethod;
        private TypeKeyGetter _keyGetter;
        private TypeKeySetter _keySetter;

        protected IBinaryStorageFile Storage { get; }

        public ref readonly StorageOptions Options => ref Storage.Options;

        public TypeToken Type => Storage.Type;

        protected StructuredSerializer(IBinaryStorageFile storage)
        {
            Storage = storage;
            if (storage.Header.IndexTable.Exists)
                SetIndexColumn(storage.Header.IndexColumn);
        }

        public void SetIndexColumn(int indexColumn)
        {
            var rootExpression = Expr.Parameter(typeof(T).MakeByRefType(), "model");
            
            var (indexColumnMemberToken, memberAccess) = Type.MakeMemberAccess(indexColumn, rootExpression, Options.TokenType);
            if (indexColumnMemberToken == null)
                throw new InvalidOperationException($"Invalid structure: Unable to find an index column.");

            if (indexColumnMemberToken.TypeToken != typeof(int) && indexColumnMemberToken.TypeToken != typeof(uint))
                throw new InvalidOperationException($"Invalid structure: {memberAccess} is expected to be the index, but its type doesn't match. Needs to be (u)int.");
            
            { /* key getter */
                _keyGetter = Expr.Lambda<TypeKeyGetter>(
                    // Box to int if type mismatches
                    memberAccess.Type == typeof(int)
                        ? memberAccess
                        : Expr.ConvertChecked(memberAccess, typeof(int)),
                    rootExpression).Compile();
            }

            { /* key setter */
                var paramValue = Expr.Parameter(typeof(int));

                _keySetter = Expr.Lambda<TypeKeySetter>(
                    Expr.Assign(memberAccess,
                        // Box to target type if not int
                        memberAccess.Type == typeof(int)
                            ? memberAccess
                            : Expr.ConvertChecked(paramValue, memberAccess.Type)
                ), rootExpression, paramValue).Compile();
            }
        }

        /// <summary>
        /// Extract the key value of a given record.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public int GetRecordKey(in T instance) => _keyGetter(in instance);
    
        /// <summary>
        /// Force-set the key of a record to the provided value.
        /// </summary>
        /// <param name="instance">The record instance to modify.</param>
        /// <param name="key">The new key value to set<</param>
        public void SetRecordKey(out T instance, int key) => _keySetter(out instance, key);

        /// <summary>
        /// Clone the provided instance.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="clonedInstance"></param>
        public void Clone(in T origin, out T clonedInstance)
        {
            if (_cloneMethod == null)
            {
                Debug.Assert(Options.MemberType == MemberTypes.Field || Options.MemberType == MemberTypes.Property);

                var oldInstanceParam = Expr.Parameter(typeof(T).MakeByRefType());
                var newInstanceParam = Expr.Parameter(typeof(T).MakeByRefType());

                var body = new List<Expr> {
                    Expr.Assign(newInstanceParam, Expr.New(typeof(T)))
                };

                foreach (var memberInfo in Type.Members)
                {
                    var oldMemberAccess = memberInfo.MakeAccess(oldInstanceParam);
                    var newMemberAccess = memberInfo.MakeAccess(newInstanceParam);

                    body.Add(CloneMember(memberInfo, oldMemberAccess, newMemberAccess));
                }

                body.Add(newInstanceParam);

                var bodyBlock = Expr.Block(body);
                _cloneMethod = Expr.Lambda<TypeCloner>(bodyBlock, oldInstanceParam, newInstanceParam).Compile();
            }

            _cloneMethod.Invoke(in origin, out clonedInstance);
        }

        private Expr CloneMember(MemberToken memberInfo, Expr oldMember, Expr newMember)
        {
            if (oldMember.Type.IsArray)
            {
                var sizeVarExpr = Expr.Variable(typeof(int));
                var lengthValue = Expr.MakeMemberAccess(oldMember,
                    oldMember.Type.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance));
                var newArrayExpr = memberInfo.TypeToken.GetElementTypeToken().NewArrayBounds(sizeVarExpr);

                var loopItr = Expr.Variable(typeof(int));
                var loopCondition = Expr.LessThan(loopItr, sizeVarExpr);
                var loopExitLabel = Expr.Label();

                return Expr.Block(new[] { loopItr, sizeVarExpr },
                    Expr.Assign(sizeVarExpr, lengthValue),
                    Expr.Assign(newMember, newArrayExpr),
                    Expr.Assign(loopItr, Expr.Constant(0)),
                    Expr.Loop(
                        Expr.IfThenElse(loopCondition,
                            Expr.Block(
                                CloneMember(memberInfo, Expr.ArrayAccess(oldMember, loopItr), Expr.ArrayAccess(newMember, loopItr)),
                                Expr.PreIncrementAssign(loopItr)
                            ),
                            Expr.Break(loopExitLabel)
                        ), loopExitLabel));
            }


            var typeInfo = Type.GetChildToken(oldMember.Type);

            if (typeInfo == typeof(string) || typeInfo.IsPrimitive)
                return Expr.Assign(newMember, oldMember);

            var block = new List<Expr>() {
                Expr.Assign(newMember, Expr.New(newMember.Type))
            };

            foreach (var childInfo in typeInfo.Members)
            {
                if (Options.TokenType != childInfo.MemberType || childInfo.IsReadOnly)
                    continue;

                var oldChild = childInfo.MakeAccess(oldMember);
                var newChild = childInfo.MakeAccess(newMember);

                block.Add(CloneMember(childInfo, oldChild, newChild));
            }

            return block.Count == 1
                ? (Expr)Expr.Assign(newMember, oldMember)
                : (Expr)Expr.Block(block);
        }
    }
}
