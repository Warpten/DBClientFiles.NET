using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DBClientFiles.NET.Parsing.File;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils;

using TypeToken = DBClientFiles.NET.Parsing.Reflection.TypeToken;

namespace DBClientFiles.NET.Parsing.Serialization
{
    internal abstract class StructuredSerializer<T> : ISerializer<T>
    {
        private Expression _keyAccessExpression;
        private ParameterExpression _root;

        protected delegate void TypeCloner(in T source, out T target);
        protected delegate void TypeDeserializer(IRecordReader reader, out T instance);
        protected delegate int TypeKeyGetter(in T source);
        protected delegate void TypeKeySetter(out T source, int key);

        private TypeCloner _cloneMethod;
        private TypeDeserializer _deserializer;
        private TypeKeyGetter _keyGetter;
        private TypeKeySetter _keySetter;

        private StorageOptions _options;
        public ref readonly StorageOptions Options {
            get => ref _options;
        }
        public TypeToken Type { get; protected set; }

        public virtual void Initialize(IBinaryStorageFile storage)
        {
            _options = storage.Options;
            Type = storage.Type;

            // TODO: Fix this, array members don't work
            SetIndexColumn(storage.Header.IndexColumn);
        }

        public void SetIndexColumn(int indexColumn)
        {
            _keyAccessExpression = _root = Expression.Parameter(typeof(T).MakeByRefType(), "model");

            var indexColumnMember = Type.GetMemberByIndex(ref indexColumn, ref _keyAccessExpression, _options.MemberType.ToTypeToken());
            if (indexColumnMember == null)
                throw new InvalidOperationException($"Invalid structure: Unable to find an index column.");

            if (indexColumnMember.TypeToken.Type != typeof(int) && indexColumnMember.TypeToken.Type != typeof(uint))
            {
                throw new InvalidOperationException($"Invalid structure: {_keyAccessExpression} is expected to be the index, but its type doesn't match. Needs to be (u)int.");
            }
        }

        /// <summary>
        /// Extract the key value of a given record.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public int GetKey(in T instance)
        {
            if (_keyGetter == null)
                _keyGetter = Expression.Lambda<TypeKeyGetter>(
                    // Box to int
                    Expression.ConvertChecked(_keyAccessExpression, typeof(int)),
                    _root).Compile();

            return _keyGetter(in instance);
        }

        /// <summary>
        /// Force-set the key of a record to the provided value.
        /// </summary>
        /// <param name="instance">The record instance to modify.</param>
        /// <param name="key">The new key value to set<</param>
        public void SetKey(out T instance, int key)
        {
            if (_keySetter == null)
            {
                var paramValue = Expression.Parameter(typeof(int));

                _keySetter = Expression.Lambda<TypeKeySetter>(
                    Expression.Assign(_keyAccessExpression, Expression.ConvertChecked(paramValue, _keyAccessExpression.Type)
                ), _root, paramValue).Compile();
            }

            _keySetter(out instance, key);
        }

        /// <summary>
        /// Clone the provided instance.
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public T Clone(in T origin)
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
                    var oldMemberAccess = Expression.MakeMemberAccess(oldInstanceParam, memberInfo.MemberInfo);
                    var newMemberAccess = Expression.MakeMemberAccess(newInstanceParam, memberInfo.MemberInfo);

                    body.Add(CloneMember(memberInfo, oldMemberAccess, newMemberAccess));
                }

                body.Add(newInstanceParam);

                var bodyBlock = Expression.Block(body);
                _cloneMethod = Expression.Lambda<TypeCloner>(bodyBlock, oldInstanceParam, newInstanceParam).Compile();
            }

            _cloneMethod.Invoke(in origin, out var instance);
            return instance;
        }

        private Expression CloneMember(MemberToken memberInfo, Expression oldMember, Expression newMember)
        {
            if (memberInfo.IsArray)
            {
                var sizeVarExpr = Expression.Variable(typeof(int));
                var lengthValue = Expression.MakeMemberAccess(oldMember,
                    oldMember.Type.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance));
                var newArrayExpr = Expression.NewArrayBounds(memberInfo.TypeToken.GetElementTypeToken().Type, sizeVarExpr);

                var loopItr = Expression.Variable(typeof(int));
                var loopCondition = Expression.LessThan(loopItr, sizeVarExpr);
                var loopExitLabel = Expression.Label();

                return Expression.Block(new[] { loopItr, sizeVarExpr },
                    Expression.Assign(sizeVarExpr, lengthValue),
                    Expression.Assign(newMember, newArrayExpr),
                    Expression.Assign(loopItr, Expression.Constant(0)),
                    Expression.Loop(
                        Expression.IfThenElse(loopCondition,
                            Expression.Block(
                                CloneMember(memberInfo, Expression.ArrayAccess(oldMember, loopItr), Expression.ArrayAccess(newMember, loopItr)),
                                Expression.PreIncrementAssign(loopItr)
                            ),
                            Expression.Break(loopExitLabel)
                        ), loopExitLabel));
            }


            var typeInfo = Type.GetChildToken(oldMember.Type);

            if (typeInfo.Type == typeof(string) || typeInfo.Type.IsPrimitive)
                return Expression.Assign(newMember, oldMember);

            var block = new List<Expression>() {
                Expression.Assign(newMember, New.Expression(newMember.Type))
            };

            foreach (var childInfo in typeInfo.Members)
            {
                if (!ShouldProcess(childInfo))
                    continue;

                var oldChild = Expression.MakeMemberAccess(oldMember, childInfo.MemberInfo);
                var newChild = Expression.MakeMemberAccess(newMember, childInfo.MemberInfo);

                block.Add(CloneMember(childInfo, oldChild, newChild));
            }

            return block.Count == 1
                ? (Expression)Expression.Assign(newMember, oldMember)
                : (Expression)Expression.Block(block);
        }

        public T Deserialize(IRecordReader reader)
        {
            if (_deserializer == null)
                _deserializer = GenerateDeserializer();

            _deserializer.Invoke(reader, out var instance);
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
                if (!ShouldProcess(memberInfo))
                    continue;

                var memberNode = Expression.MakeMemberAccess(typeVariable, memberInfo.MemberInfo);
                Visit(body, reader, memberInfo, memberNode);
            }

            var bodyBlock = Expression.Block(body);

            var lambda = Expression.Lambda<TypeDeserializer>(bodyBlock, reader, typeVariable);
            return lambda.Compile();
        }

        protected virtual bool ShouldProcess(MemberToken memberInfo)
        {
            if (Options.MemberType == MemberTypes.Field)
            {
                if (memberInfo.MemberType != TypeTokenType.Field)
                    return false;
            }

            if (Options.MemberType == MemberTypes.Property)
            {
                if (memberInfo.MemberType != TypeTokenType.Property)
                    return false;
            }

            return !memberInfo.IsReadOnly;
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
        public void Visit(List<Expression> container, Expression recordReader, MemberToken memberInfo, Expression memberAccess)
        {
            if (memberInfo.IsArray)
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
                    foreach (var childInfo in Type.Members)
                    {
                        if (!ShouldProcess(memberInfo))
                            continue;

                        var childAccess = Expression.MakeMemberAccess(arrayElement, childInfo.MemberInfo);
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
                            ), breakLabelTarget)));
                }
            }
            else
            {
                var nodeInitializer = VisitNode(memberAccess, memberInfo, recordReader);
                if (nodeInitializer != null)
                    container.Add(Expression.Assign(memberAccess, nodeInitializer));
                else if (memberInfo.TypeToken.Type.IsClass)
                    container.Add(Expression.Assign(memberAccess, Expression.New(memberInfo.TypeToken.Type)));

                foreach (var child in memberInfo.TypeToken.Members)
                {
                    if (!ShouldProcess(memberInfo))
                        continue;

                    var childAccess = Expression.MakeMemberAccess(memberAccess, child.MemberInfo);
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
        public abstract Expression VisitNode(Expression memberAccess, MemberToken memberInfo, Expression recordReader);
    }
}
