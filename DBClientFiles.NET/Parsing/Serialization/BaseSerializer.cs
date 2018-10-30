using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            _indexColumn = typeInfo.EnumerateFlat().ElementAtOrDefault(indexColumn);
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

                foreach (var memberInfo in Type.Members)
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
            foreach (var memberInfo in Type.Members)
            {
                var memberNode = memberInfo.MakeMemberAccess(typeVariable);

                Visit(body, reader, memberNode);
            }

            var bodyBlock = Expression.Block(body);

            var lambda = Expression.Lambda<TypeDeserializer>(bodyBlock, reader, typeVariable);
            return lambda.Compile();
        }

        /// <summary>
        /// <para>This method is hell.</para>
        /// <para>
        /// For a given node, it recursively visits all of its children, and produces expressions for deserialization.
        /// That's really all you need to know. Deserializing primitives such as integers and string falls on the implementations
        /// of this class (via <see cref="VisitNode(ExtendedMemberExpression, Expression)"/>.
        /// </para>
        /// </summary>
        /// <param name="recordReader">An expression representing an instance of <see cref="IRecordReader"/>.</param>
        /// <param name="rootNode">The node from which we starting work down.</param>
        /// <returns></returns>
        /// <remarks>
        /// Custom types with a constructor taking an instance of <see cref="IRecordReader"/> as their unique argument
        /// cause the entire evaluation to cut off short, and a call to that constructor is emitted instead. This means
        /// that it is the responsability of said constructor to properly initialize any substructure it may contain.
        /// </remarks>
        public void Visit(List<Expression> container, Expression recordReader, ExtendedMemberExpression rootNode)
        {
            var memberType = rootNode.MemberInfo.Type;
            var elementType = memberType.IsArray ? memberType.GetElementType() : memberType;

            var memberAccess = rootNode;

            var constructorInfo = elementType.GetConstructor(new[] { typeof(IRecordReader) });
            if (memberType.IsArray)
            {
                if (constructorInfo != null)
                {
                    var arrayExpr = Expression.NewArrayBounds(elementType, Expression.Constant(rootNode.MemberInfo.Cardinality));
                    container.Add(Expression.Assign(memberAccess.Expression, arrayExpr));

                    var breakLabelTarget = Expression.Label();

                    var itr = Expression.Variable(typeof(int));
                    var condition = Expression.LessThan(itr, Expression.Constant(rootNode.MemberInfo.Cardinality));

                    container.Add(Expression.Loop(Expression.Block(new[] { itr }, new Expression[] {
                        Expression.Assign(itr, Expression.Constant(0)),
                        Expression.IfThenElse(condition,
                            Expression.Assign(
                                Expression.ArrayIndex(memberAccess.Expression, Expression.PostIncrementAssign(itr)),
                                Expression.New(constructorInfo, recordReader)),
                            Expression.Break(breakLabelTarget))
                    }), breakLabelTarget));
                }
                else
                {
                    var nodeInitializer = VisitNode(memberAccess, recordReader);
                    if (nodeInitializer != null)
                        container.Add(nodeInitializer);

                    var arrayMemberInfo = memberAccess.MemberInfo.Children[0];
                    if (arrayMemberInfo.Children.Count != 0)
                    {
                        var breakLabelTarget = Expression.Label();

                        var itr = Expression.Variable(typeof(int));
                        var arrayBound = Expression.Constant(memberAccess.MemberInfo.Cardinality);
                        var loopTest = Expression.LessThan(itr, arrayBound);

                        var arrayElement = Expression.ArrayAccess(memberAccess.Expression, itr);

                        var loopBody = new List<Expression>();
                        foreach (var childInfo in arrayMemberInfo.Children)
                        {
                            var childAccess = childInfo.MakeMemberAccess(arrayElement);
                            Visit(loopBody, recordReader, childAccess);
                        }

                        container.Add(Expression.Block(new[] { itr },
                            Expression.Assign(itr, Expression.Constant(0)),
                            Expression.Loop(
                                Expression.IfThenElse(loopTest,
                                    Expression.Block(
                                        Expression.Assign(
                                            arrayElement,
                                            New.Expression(arrayMemberInfo.Type)
                                        ),
                                        Expression.Block(loopBody),
                                        Expression.PreIncrementAssign(itr)
                                    ),
                                    Expression.Break(breakLabelTarget)
                                )
                            , breakLabelTarget)));
                    }
                }
            }
            else if (constructorInfo != null)
            {
                container.Add(Expression.Assign(memberAccess.Expression, Expression.New(constructorInfo, recordReader)));
            }
            else
            {
                if (memberAccess.MemberInfo.Type.IsClass)
                    container.Add(Expression.Assign(memberAccess.Expression, Expression.New(memberAccess.MemberInfo.Type)));

                var nodeInitializer = VisitNode(memberAccess, recordReader);
                if (nodeInitializer != null)
                    container.Add(nodeInitializer);

                foreach (var child in memberAccess.MemberInfo.Children)
                {
                    var childAccess = child.MakeMemberAccess(rootNode.Expression);
                    Visit(container, recordReader, childAccess);
                }
            }
        }

        /// <summary>
        /// This method is invoked on primitive members such as integers, floats, but also on arrays of these.
        /// </summary>
        /// <param name="memberAccess"></param>
        /// <param name="recordReader"></param>
        public abstract Expression VisitNode(ExtendedMemberExpression memberAccess, Expression recordReader);
    }
}
