using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils;

using TypeInfo = DBClientFiles.NET.Parsing.Reflection.TypeInfo;

namespace DBClientFiles.NET.Parsing.Serialization
{
    internal abstract class BaseSerializer<TKey, TValue> : BaseSerializer<TValue>, ISerializer<TKey, TValue>
    {
        private Func<TValue, TKey> _keyGetter;
        private Action<TValue, TKey> _keySetter;

        private Member _indexColumn;

        public BaseSerializer(StorageOptions options, int indexColumn) : this(options, null, indexColumn)
        {
        }

        public BaseSerializer(StorageOptions options, TypeInfo typeInfo, int indexColumn) : base(options, typeInfo)
        {
            _indexColumn = typeInfo.GetMemberByIndex(indexColumn, options.MemberType);
            if (_indexColumn == null)
                throw new ArgumentException($"Invalid column index provided to BaseSerializer<{typeof(TKey).Name}, {typeof(TValue).Name}>.");
            else if (typeof(string).IsAssignableFrom(_indexColumn.Type.Type))
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

            return Expression.Lambda<Func<TValue, TKey>>(_indexColumn.MakeAccess(param), param).Compile();
        }

        private Action<TValue, TKey> GenerateKeySetter()
        {
            var paramType = Expression.Parameter(typeof(TValue));
            var paramValue = Expression.Parameter(typeof(TKey));

            return Expression.Lambda<Action<TValue, TKey>>(
                Expression.Assign(_indexColumn.MakeAccess(paramType), paramValue
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

                var oldInstanceParam = Expression.Parameter(typeof(T).MakeByRefType());
                var newInstanceParam = Expression.Parameter(typeof(T).MakeByRefType());

                var body = new List<Expression> {
                    Expression.Assign(newInstanceParam, New<T>.Expression())
                };

                foreach (var memberInfo in Type.Members)
                {
                    var oldMemberAccessExpr = memberInfo.MakeAccess(oldInstanceParam);
                    var newMemberAccessExpr = memberInfo.MakeAccess(newInstanceParam);

                    body.Add(CloneMember(memberInfo, oldMemberAccessExpr, newMemberAccessExpr));
                }

                body.Add(newInstanceParam);

                var bodyBlock = Expression.Block(body);
                _cloneMethod = Expression.Lambda<TypeCloner>(bodyBlock, oldInstanceParam, newInstanceParam).Compile();
            }

            var instance = New<T>.Instance();
            _cloneMethod.Invoke(ref origin, ref instance);
            return instance;
        }

        private Expression CloneMember(Member field, Expression oldMember, Expression newMember)
        {
            if (oldMember.Type.IsArray)
            {
                var sizeExpression = Expression.Variable(typeof(int));
                var sizeExprValue = Expression.MakeMemberAccess(oldMember, oldMember.Type.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance));
                var newArrayExpr = Expression.NewArrayBounds(newMember.Type.GetElementType(), sizeExpression);

                var loopItr = Expression.Variable(typeof(int));
                var loopCondition = Expression.LessThan(loopItr, sizeExpression);
                var loopExitLabel = Expression.Label();

                return Expression.Block(new[] { loopItr, sizeExpression },
                    // sizeExpression = oldStructure.<FIELD>.Length;
                    Expression.Assign(sizeExpression, sizeExprValue),
                    // newStructure.<FIELD> = new T[sizeExpression]
                    Expression.Assign(newMember, newArrayExpr),
                    // itr = 0
                    Expression.Assign(loopItr, Expression.Constant(0)),

                    Expression.Loop(
                        Expression.IfThenElse(loopCondition,
                            Expression.Block(
                                CloneMember(field, Expression.ArrayAccess(oldMember, loopItr), Expression.ArrayAccess(newMember, loopItr)),
                                Expression.PreIncrementAssign(loopItr)
                            ),
                            Expression.Break(loopExitLabel)
                        ), loopExitLabel));
            }
            else
            {
                var typeInfo = field.DeclaringType.GetChildTypeInfo(oldMember.Type);
                if (typeInfo.HasChildren)
                {
                    var block = new List<Expression>() {
                        Expression.Assign(newMember, New.Expression(newMember.Type))
                    };

                    foreach (var childInfo in typeInfo.Members)
                    {
                        if (childInfo.MemberType != Options.MemberType)
                            continue;

                        var oldChild = childInfo.MakeAccess(oldMember);
                        var newChild = childInfo.MakeAccess(newMember);

                        block.Add(CloneMember(childInfo, oldChild, newChild));
                    }

                    return Expression.Block(block);
                }
                else
                    return Expression.Assign(newMember, oldMember);
            }
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
                if (memberInfo.MemberType != Options.MemberType || memberInfo.IsReadOnly)
                    continue;

                var memberNode = memberInfo.MakeAccess(typeVariable);
                Visit(body, reader, memberInfo, memberNode);
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
        public void Visit(List<Expression> container, Expression recordReader, Member memberInfo, Expression memberAccess)
        {
            if (memberInfo.Type.ElementTypeInfo != null)
            {
                // Try to read it using the visiters if it's a simple POD type.
                var nodeInitializer = VisitNode(memberAccess, memberInfo, recordReader);
                if (nodeInitializer != null)
                    container.Add(Expression.Assign(memberAccess, nodeInitializer));
                else
                {
                    // Visitors couldn't help (they only handle basic types)
                    // This is an array of complex objects and needs manual looping.
                    var breakLabelTarget = Expression.Label();

                    var itr = Expression.Variable(typeof(int));
                    var arrayElement = Expression.ArrayAccess(memberAccess, itr);

                    var arrayBound = Expression.Constant(memberInfo.Cardinality);
                    var loopTest = Expression.LessThan(itr, arrayBound);

                    // Construct the loop's body
                    var loopBody = new List<Expression>();
                    foreach (var childInfo in Type.ElementTypeInfo.Members)
                    {
                        if (childInfo.MemberType != memberInfo.MemberType || childInfo.IsReadOnly)
                            continue;

                        var childAccess = childInfo.MakeAccess(arrayElement);
                        Visit(loopBody, recordReader, childInfo, childAccess);
                    }

                    // And now construct the loop itself
                    container.Add(Expression.Block(new[] { itr },
                        Expression.Assign(itr, Expression.Constant(0)),
                        Expression.Loop(
                            Expression.IfThenElse(loopTest,
                                Expression.Block(
                                    Expression.Assign(arrayElement, New.Expression(arrayElement.Type)),
                                    Expression.Block(loopBody),
                                    Expression.PreIncrementAssign(itr)
                                ),
                                Expression.Break(breakLabelTarget)
                            ),
                        breakLabelTarget)));
                }
            }
            else
            {

                var nodeInitializer = VisitNode(memberAccess, memberInfo, recordReader);
                if (nodeInitializer != null)
                    container.Add(Expression.Assign(memberAccess, nodeInitializer));
                else if (memberInfo.Type.IsClass)
                    container.Add(Expression.Assign(memberAccess, Expression.New(memberInfo.Type.Type)));

                foreach (var child in memberInfo.Type.Members)
                {
                    if (child.IsReadOnly || child.MemberType != memberInfo.MemberType)
                        continue;

                    var childAccess = child.MakeAccess(memberAccess);
                    Visit(container, recordReader, child, childAccess);
                }
            }
        }

        /// <summary>
        /// This method is invoked on primitive members such as integers, floats, but also on arrays of these.
        /// </summary>
        /// <param name="memberAccess"></param>
        /// <param name="memberInfo"></param>
        /// <param name="recordReader"></param>
        public abstract Expression VisitNode(Expression memberAccess, Member memberInfo, Expression recordReader);
    }
}
