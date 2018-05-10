using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Internals.Versions;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Internals.Serializers
{
    /// <summary>
    /// A specialization of the above that has key capabilities.
    /// </summary>
    /// <inheritdoc/>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class LegacySerializer<TKey, TValue> : LegacySerializer<TValue> where TValue : class, new()
    {
        private static Action<TValue, TKey> _keySetter;
        private static Func<TValue, TKey> _keyGetter;

        public LegacySerializer(BaseReader<TValue> storage) : base(storage) { }

        protected virtual bool IsMemberKey(MemberInfo memberInfo) => memberInfo.CustomAttributes.Any(attr => attr.AttributeType == typeof(IndexAttribute));

        public void InsertKey(TValue value, TKey newKey)
        {
            if (_keySetter == null)
            {
                MemberInfo keyMemberInfo = null;

                foreach (var memberInfo in typeof(TValue).GetMembers(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (memberInfo.MemberType != Options.MemberType)
                        continue;

                    // Get the first member available as a fallback for some implementations
                    if (keyMemberInfo == null)
                        keyMemberInfo = memberInfo;

                    if (IsMemberKey(memberInfo))
                    {
                        keyMemberInfo = memberInfo;
                        break;
                    }
                }

                if (keyMemberInfo == null)
                    throw new InvalidOperationException("Unable to find a key column for type `" + typeof(TValue).Name + "`.");

                var newKeyArgExpr = Expression.Parameter(typeof(TKey), "key");
                var recordArgExpr = Expression.Parameter(typeof(TValue), "record");
                var memberAccessExpr = Expression.MakeMemberAccess(recordArgExpr, keyMemberInfo);
                var assignmentExpr = Expression.Assign(memberAccessExpr, newKeyArgExpr);
                _keySetter = Expression.Lambda<Action<TValue, TKey>>(assignmentExpr, new[] { recordArgExpr, newKeyArgExpr }).Compile();
            }

            _keySetter(value, newKey);
        }

        public TKey ExtractKey(TValue value)
        {
            if (_keyGetter == null)
            {
                MemberInfo keyMemberInfo = null;

                foreach (var memberInfo in typeof(TValue).GetMembers(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (memberInfo.MemberType != Options.MemberType)
                        continue;

                    // Get the first member available as a fallback for some implementations
                    if (keyMemberInfo == null)
                        keyMemberInfo = memberInfo;

                    if (IsMemberKey(memberInfo))
                    {
                        keyMemberInfo = memberInfo;
                        break;
                    }
                }

                if (keyMemberInfo == null)
                    throw new InvalidOperationException("Unable to find a key column for type `" + typeof(TValue).Name + "`.");

                var recordArgExpr = Expression.Parameter(typeof(TValue), "record");
                var memberAccessExpr = Expression.MakeMemberAccess(recordArgExpr, keyMemberInfo);
                _keyGetter = Expression.Lambda<Func<TValue, TKey>>(memberAccessExpr, new[] { recordArgExpr }).Compile();
            }

            return _keyGetter(value);
        }
    }

    /// <summary>
    /// A basic serializer that supports most basic DBC formats such as WDBC, WDB2, WDB5.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    internal class LegacySerializer<TValue> where TValue : class, new()
    {
        protected StorageOptions Options => Storage.Options;

        private Func<TValue, TValue> _memberwiseClone;
        private Func<BinaryReader, TValue> _deserializer;

        protected BaseReader<TValue> Storage { get; }

        public LegacySerializer(BaseReader<TValue> storage)
        {
            Storage = storage;
        }

        public TValue Clone(TValue source)
        {
            if (_memberwiseClone == null)
            {
                var body = new List<Expression>();

                var inputNode = Expression.Parameter(typeof(TValue));
                var outputNode = Expression.Variable(typeof(TValue));
                var newNode = Expression.New(typeof(TValue));

                body.Add(Expression.Assign(outputNode, newNode));

                foreach (var memberInfo in typeof(TValue).GetMembers(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (memberInfo.MemberType != Options.MemberType)
                        continue;

                    var oldMember = Expression.MakeMemberAccess(inputNode, memberInfo);
                    var newMember = Expression.MakeMemberAccess(outputNode, memberInfo);
                    body.Add(Expression.Assign(newMember, oldMember));
                }

                body.Add(outputNode);
                var block = Expression.Block(new[] { outputNode }, body);
                var lmbda = Expression.Lambda<Func<TValue, TValue>>(block, inputNode);
                _memberwiseClone = lmbda.Compile();
            }

            return _memberwiseClone(source);
        }

        protected virtual Func<BinaryReader, TValue> GenerateDeserializer()
        {
            var readerArgExpr = Expression.Parameter(typeof(BinaryReader));
            var resultExpr = Expression.Variable(typeof(TValue));

            var instanceExpr = Expression.Assign(resultExpr, Expression.New(typeof(TValue)));
            var body = new List<Expression> {
                instanceExpr
            };

            var memberIndex = 0;
            foreach (var memberInfo in typeof(TValue).GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (memberInfo.MemberType != Options.MemberType)
                    continue;

                var memberAccessExpr = memberInfo.MakeMemberAccess(resultExpr);
                var memberType = memberInfo.GetMemberType();

                if (!CanSerializeMember(memberIndex++, memberInfo))
                    continue;

                var methodInfo = memberType.GetReaderMethod();
                Expression methodCallExpr;

                if (methodInfo == null)
                {
                    var ctorInfo = memberType.GetConstructor(new[] { typeof(BinaryReader) });
                    if (ctorInfo == null)
                        throw new InvalidOperationException($@"Type '{memberType.Name}' requires a ctor(BinaryReader) to be used in (de)serialization!");
                    methodCallExpr = Expression.New(ctorInfo, readerArgExpr);
                }
                else
                    methodCallExpr = Expression.Call(readerArgExpr, methodInfo);

                if (!memberType.IsArray)
                {
                    body.Add(Expression.Assign(memberAccessExpr, methodCallExpr));
                }
                else
                {
                    var arraySize = memberInfo.GetArraySize();

                    body.Add(Expression.Assign(memberAccessExpr, Expression.NewArrayBounds(memberType.GetElementType(), Expression.Constant(arraySize))));

                    for (var i = 0; i < arraySize; ++i)
                    {
                        var arrayMember = Expression.ArrayAccess(memberAccessExpr, Expression.Constant(i));
                        var assignment = Expression.Assign(arrayMember, methodCallExpr);
                        body.Add(assignment);
                    }
                }
            }

            body.Add(resultExpr);

            var bodyExpr = Expression.Block(new[] { resultExpr }, body);
            var fnExpr = Expression.Lambda<Func<BinaryReader, TValue>>(bodyExpr, new[] { readerArgExpr });
            return fnExpr.Compile();
        }

        protected virtual Expression GetMemberBaseReadExpression(MemberInfo memberInfo, Expression readerInstance)
        {
            var memberType = memberInfo.GetMemberType();
            var methodInfo = memberType.GetReaderMethod();

            if (methodInfo == null)
            {
                var ctorInfo = memberType.GetConstructor(new[] { typeof(BinaryReader) });
                if (ctorInfo == null)
                    throw new InvalidOperationException($@"Type '{memberType.Name}' requires a ctor(BinaryReader) to be used in (de)serialization!");
                return Expression.New(ctorInfo, readerInstance);
            }
            else
                return Expression.Call(readerInstance, methodInfo);
        }

        protected virtual bool CanSerializeMember(int memberIndex, MemberInfo memberInfo) => memberInfo.GetCustomAttribute<IgnoreAttribute>() != null;

        protected virtual bool IsCommonDataMember(int memberIndex, MemberInfo memberInfo) => false;
        protected virtual bool IsPalletDataMember(int memberIndex, MemberInfo memberInfo) => false;
        protected virtual bool IsRelationShipDataMember(int memberIndex, MemberInfo memberInfo) => false;

        public virtual TValue Deserialize(BinaryReader reader)
        {
            if (_deserializer == null)
                _deserializer = GenerateDeserializer();

            return _deserializer(reader);
        }
    }
}
