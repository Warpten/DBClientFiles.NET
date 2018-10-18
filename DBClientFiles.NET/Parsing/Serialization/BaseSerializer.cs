using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File;
using DBClientFiles.NET.Parsing.Types;
using DBClientFiles.NET.Utils;

using TypeInfo = DBClientFiles.NET.Parsing.Types.TypeInfo;

namespace DBClientFiles.NET.Parsing.Serialization
{
    internal abstract class BaseSerializer<TKey, TValue> : BaseSerializer<TValue>, ISerializer<TKey, TValue>
    {
        private Func<TValue, TKey> _keyGetter;
        private Action<TValue, TKey> _keySetter;

        private ITypeMember _indexColumn;

        public BaseSerializer(StorageOptions options, int indexColumn) : this(options, null, indexColumn)
        {
        }

        public BaseSerializer(StorageOptions options, TypeInfo typeInfo, int indexColumn) : base(options, typeInfo)
        {
            _indexColumn = typeInfo.EnumerateFlat(options.MemberType).ElementAtOrDefault(indexColumn);
            if (_indexColumn == null)
                throw new ArgumentException($"Invalid column index provided to BaseSerializer<{typeof(TKey).Name}, {typeof(TValue).Name}>.");
            else if (typeof(string).IsAssignableFrom(_indexColumn.Type))
                throw new ArgumentException($"Column index provided to BaseSerializer<{typeof(TKey).Name}, {typeof(TValue).Name}> points to a string ({_indexColumn.MemberInfo.Name}).");
        }

        /// <summary>
        /// Extract the key value of a given record.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public TKey GetKey(TValue instance)
        {
            if (_keyGetter == null)
                _keyGetter = GenerateKeyGetter();

            return _keyGetter(instance);
        }

        /// <summary>
        /// Force-set the key value if a record to the provided value.
        /// </summary>
        /// <param name="instance">The record instance to modify.</param>
        /// <param name="key">The new key value to set<</param>
        public void SetKey(TValue instance, TKey key)
        {
            if (_keySetter == null)
                _keySetter = GenerateKeySetter();

            _keySetter(instance, key);
        }

        private Func<TValue, TKey> GenerateKeyGetter()
        {
            var param = Expression.Parameter(typeof(TValue));

            return Expression.Lambda<Func<TValue, TKey>>(_indexColumn.MakeMemberAccess(param).Expression, param).Compile();
        }

        private Action<TValue, TKey> GenerateKeySetter()
        {
            var paramType = Expression.Parameter(typeof(TValue));
            var paramValue = Expression.Parameter(typeof(TKey));

            return Expression.Lambda<Action<TValue, TKey>>(
                Expression.Assign(_indexColumn.MakeMemberAccess(paramType).Expression, paramValue
            ), paramType, paramValue).Compile();
        }
    }

    internal abstract class BaseSerializer<T> : ISerializer<T>
    {
        protected delegate void TypeCloner(ref T source, ref T target);
        protected delegate void TypeDeserializer(IRecordReader reader, ref T instance);

        private TypeCloner _cloneMethod;
        private TypeDeserializer _deserializer;

        public StorageOptions Options { get; }

        public TypeInfo Type { get; }

        public BaseSerializer(StorageOptions options, TypeInfo memberInfo)
        {
            Debug.Assert(memberInfo != null);

            Options = options;
            Type = memberInfo;
        }

        /// <summary>
        /// Balls-to-the-wall implementation of type-safe, fast, deep cloning.
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public T Clone(T origin)
        {
            if (_cloneMethod == null)
            {
                Debug.Assert(Options.MemberType == MemberTypes.Field || Options.MemberType == MemberTypes.Property);

                var body = new List<Expression>();

                var oldInstanceParam = Expression.Parameter(typeof(T).MakeByRefType());
                var newInstanceParam = Expression.Parameter(typeof(T).MakeByRefType());

                body.Add(Expression.Assign(newInstanceParam, New<T>.Expression()));

                foreach (var memberInfo in Type.Enumerate(Options.MemberType))
                {
                    var oldMemberAccessExpr = memberInfo.MakeMemberAccess(oldInstanceParam).Expression;
                    var newMemberAccessExpr = memberInfo.MakeMemberAccess(newInstanceParam).Expression;

                    body.Add(RecursiveMemberClone(memberInfo, oldMemberAccessExpr, newMemberAccessExpr));
                }

                body.Add(newInstanceParam);

                var bodyBlock = Expression.Block(body);
                _cloneMethod = Expression.Lambda<TypeCloner>(bodyBlock, oldInstanceParam, newInstanceParam).Compile();
            }

            var instance = New<T>.Instance();
            _cloneMethod.Invoke(ref origin, ref instance);
            return instance;
        }

        private static Expression RecursiveMemberClone(ITypeMember memberInfo, Expression oldMember, Expression newMember)
        {
            if (memberInfo.Type.IsArray)
            {
                var newArrayExpr = Expression.NewArrayBounds(memberInfo.Type.GetElementType(), Expression.Constant(memberInfo.Cardinality));

                var loopItr = Expression.Variable(typeof(int));
                var loopCondition = Expression.Constant(memberInfo.Cardinality);
                var breakLabel = Expression.Label();

                return Expression.Block(new[] { loopItr },
                    Expression.Assign(newMember, newArrayExpr),
                    Expression.Assign(loopItr, Expression.Constant(0)),
                    Expression.Loop(
                        Expression.IfThenElse(
                            Expression.LessThan(loopItr, loopCondition),
                                Expression.Block(
                                    Expression.Block(RecursiveMemberClone(memberInfo.Children[0],
                                        Expression.ArrayAccess(oldMember, loopItr),
                                        Expression.ArrayAccess(oldMember, loopItr)
                                    )),
                                    Expression.PreIncrementAssign(loopItr)
                                ).Reduce(),
                            Expression.Break(breakLabel)
                        ),
                    breakLabel));
            }
            else if (memberInfo.Children.Count != 0)
            {
                Debug.Assert(!memberInfo.IsArray);

                var block = new List<Expression>() {
                    Expression.Assign(newMember, New.Expression(memberInfo.Type))
                };
                foreach (var childInfo in memberInfo.Children)
                {
                    var oldChild = Expression.MakeMemberAccess(oldMember, childInfo.MemberInfo);
                    var newChild = Expression.MakeMemberAccess(newMember, childInfo.MemberInfo);

                    block.Add(RecursiveMemberClone(childInfo, oldChild, newChild));
                }

                return Expression.Block(block);
            }
            else
                return Expression.Assign(newMember, oldMember);
        }

        public T Deserialize(IRecordReader reader)
        {
            if (_deserializer == null)
                _deserializer = GenerateDeserializer();

            var instance = New<T>.Instance();
            _deserializer.Invoke(reader, ref instance);
            return instance;
        }

        protected virtual TypeDeserializer GenerateDeserializer()
        {
            var body = new List<Expression>();

            var reader = Expression.Parameter(typeof(IRecordReader));

            var typeVariable = Expression.Parameter(typeof(T).MakeByRefType());
            var typeInstance = New<T>.Expression();
            body.Add(Expression.Assign(typeVariable, typeInstance));

            // Initialize all the substructures
            foreach (var memberInfo in Type.Enumerate(Options.MemberType))
            {
                var memberNode = memberInfo.MakeMemberAccess(typeVariable);

                var memberSerializer = GetMemberSerializer(memberInfo);
                body.AddRange(memberSerializer.Visit(reader, memberNode));
            }

            var bodyBlock = Expression.Block(body);

            var lambda = Expression.Lambda<TypeDeserializer>(bodyBlock, reader, typeVariable);
            return lambda.Compile();
        }

        /// <summary>
        /// Given an instance of <see cref="ExtendedMemberInfo"/>, provides an implementation of <see cref="IMemberSerializer"/>
        /// that is in charge of producting expressions deserializing a structure's member, no matter how nested it is.
        /// </summary>
        protected abstract IMemberSerializer GetMemberSerializer(ITypeMember memberInfo);
    }
}
