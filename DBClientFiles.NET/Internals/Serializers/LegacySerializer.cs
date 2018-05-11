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
        private Func<BaseReader<TValue>, TValue> _deserializer;

        protected BaseReader<TValue> Storage { get; }

        public LegacySerializer(BaseReader<TValue> storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Produces a deep copy of the provided object.
        /// </summary>
        /// <remarks>On the initial call, a generator function is emitted through <see cref="Linq.Expressions"/>.</remarks>
        /// <param name="source"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Generates the deserializer.
        /// </summary>
        /// <remarks>
        /// Likely not to be overriden, but let's keep it safe.
        /// </remarks>
        /// <returns></returns>
        protected virtual Func<BaseReader<TValue>, TValue> GenerateDeserializer()
        {
            var binaryReaderExpr = Expression.Parameter(typeof(BaseReader<TValue>));
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

                var extendedMemberInfo = ExtendedMemberInfo.Initialize(memberInfo, memberIndex++);
                if (extendedMemberInfo == null || !CanSerializeMember(extendedMemberInfo))
                    continue;

                var memberAccessExpr = extendedMemberInfo.MakeMemberAccess(resultExpr);
                var methodInfo = extendedMemberInfo.BinaryReader;

                var isPalletData = IsPalletDataMember(memberIndex, memberInfo);
                var isCommonData = IsCommonDataMember(memberIndex, memberInfo);
                var isRelationshipData = IsRelationShipDataMember(memberIndex, memberInfo);

                if (isPalletData)
                    GeneratePalletDataReader(body, ref memberAccessExpr, binaryReaderExpr);
                else if (isCommonData)
                    GenerateCommonDataReader(body, ref memberAccessExpr, binaryReaderExpr);
                else if (isRelationshipData)
                    GenerateRelationshipDataReader(body, ref memberAccessExpr, binaryReaderExpr);
                else
                    GenerateStreamedMemberReader(body, ref memberAccessExpr, binaryReaderExpr);
            }

            body.Add(resultExpr);

            var bodyExpr = Expression.Block(new[] { resultExpr }, body);
            var fnExpr = Expression.Lambda<Func<BaseReader<TValue>, TValue>>(bodyExpr, new[] { binaryReaderExpr });
            return fnExpr.Compile();
        }

        protected virtual void GenerateCommonDataReader(List<Expression> body, ref ExtendedMemberExpression memberExpression, Expression binaryReaderExpr)
        {
            throw new NotImplementedException();
        }

        protected virtual void GeneratePalletDataReader(List<Expression> body, ref ExtendedMemberExpression memberExpression, Expression binaryReaderExpr)
        {
            throw new NotImplementedException();
        }

        protected virtual void GenerateRelationshipDataReader(List<Expression> body, ref ExtendedMemberExpression memberExpression, Expression binaryReaderExpr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates the basic property readers - this is used in almost all legacy code and just reads plain old data directly from the file.
        ///
        /// In short, it yields code similiar to <pre>structInstance.memberField = reader.ReadInt32()</pre>. Loops are unrolled.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="memberExpression"></param>
        /// <param name="binaryReaderExpr"></param>
        private void GenerateStreamedMemberReader(List<Expression> body, ref ExtendedMemberExpression memberExpression, Expression binaryReaderExpr)
        {
            var simpleReadExpression = GetMemberBaseReadExpression(memberExpression.MemberInfo, binaryReaderExpr);

            if (!memberExpression.MemberInfo.IsArray)
            {
                body.Add(Expression.Assign(memberExpression.MemberExpression, simpleReadExpression));
            }
            else
            {
                body.Add(Expression.Assign(
                    memberExpression.MemberExpression,
                    Expression.NewArrayBounds(memberExpression.MemberInfo.ElementType.GetElementType(), Expression.Constant(memberExpression.MemberInfo.ArraySize))));

                for (var i = 0; i < memberExpression.MemberInfo.ArraySize; ++i)
                {
                    // TODO: Benchmark against expression loops.

                    var arrayMember = Expression.ArrayAccess(memberExpression.MemberExpression, Expression.Constant(i));
                    var assignment = Expression.Assign(arrayMember, simpleReadExpression);
                    body.Add(assignment);
                }
            }
        }

        /// <summary>
        /// Generates a basic in-stream reader expression, such as
        /// <pre>reader.ReadInt32()</pre>.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="readerInstance"></param>
        /// <returns></returns>
        protected virtual Expression GetMemberBaseReadExpression(ExtendedMemberInfo memberInfo, Expression readerInstance)
        {
            var memberType = memberInfo.ElementType;
            var methodInfo = memberInfo.BinaryReader;

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

        protected virtual bool CanSerializeMember(ExtendedMemberInfo memberInfo) => memberInfo.IsDefined(typeof(IgnoreAttribute), false);

        /// <summary>
        /// Returns true if this column's value is to be read from the common data block.
        /// </summary>
        /// <param name="memberIndex"></param>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        protected virtual bool IsCommonDataMember(int memberIndex, MemberInfo memberInfo) => false;

        /// <summary>
        /// Returns true if this column's value is to be read from the pallet data block.
        /// </summary>
        /// <param name="memberIndex"></param>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        protected virtual bool IsPalletDataMember(int memberIndex, MemberInfo memberInfo) => false;

        /// <summary>
        /// Returns true if this column's value is to be read from the relationship data block.
        /// </summary>
        /// <param name="memberIndex"></param>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        protected virtual bool IsRelationShipDataMember(int memberIndex, MemberInfo memberInfo) => false;

        public virtual TValue Deserialize()
        {
            if (_deserializer == null)
                _deserializer = GenerateDeserializer();

            return _deserializer(Storage);
        }
    }
}
