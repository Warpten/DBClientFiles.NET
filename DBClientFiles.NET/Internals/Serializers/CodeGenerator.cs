using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Data;
using DBClientFiles.NET.Internals.Versions;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DBClientFiles.NET.Internals.Serializers
{
    internal class CodeGenerator<T, TKey> : CodeGenerator<T>
        where T : class, new()
    {
        private Func<T, TKey> _keyGetter;
        private Action<T, TKey> _keySetter;

        #region Life and death
        public CodeGenerator(ExtendedMemberInfo[] memberInfos) : base(memberInfos)
        {
        }
        #endregion

        /// <summary>
        /// Extracts the key of a given object instance.
        /// </summary>
        /// <remarks>This will be made type-safe in the future, to discard any boxing needed.</remarks>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public TKey ExtractKey(T instance)
        {
            if (_keyGetter == null)
                _keyGetter = GenerateKeyGetter();

            return _keyGetter(instance);
        }

        /// <summary>
        /// Sets the value of the member of the provided instance that is supposed to behave like a key.
        /// </summary>
        /// <param name="instance">The target instance of <see cref="{T}"/></param>
        /// <param name="key">The value to assign to the member representating a key.</param>
        public void InsertKey(T instance, TKey key)
        {
            if (_keySetter != null)
                _keySetter = GenerateKeySetter();

            _keySetter(instance, key);
        }

        /// <summary>
        /// This method is used to determine wether or not the provided member describes a key.
        /// Ideally, this method should return true for only one member.
        /// 
        /// By default, it checks for presence of the <see cref="IndexAttribute"/> attribute.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public virtual bool IsMemberKey(ExtendedMemberInfo memberInfo) => memberInfo.IsDefined(typeof(IndexAttribute), false) || (memberInfo.MemberIndex == IndexColumn);

        /// <summary>
        /// Generates a key extractor method.
        /// </summary>
        /// <returns></returns>
        public Func<T, TKey> GenerateKeyGetter()
        {
            ExtendedMemberInfo keyMemberInfo = null;

            foreach (var memberInfo in Members)
            {
                if (IsMemberKey(memberInfo))
                {
                    keyMemberInfo = memberInfo;
                    break;
                }
            }

            if (keyMemberInfo == null)
                throw new InvalidOperationException("Unable to find a key column for type `" + typeof(T).Name + "`.");

            var recordArgExpr = Expression.Parameter(typeof(T), "record");
            var memberAccessExpr = Expression.MakeMemberAccess(recordArgExpr, keyMemberInfo.MemberInfo);
            _keyGetter = Expression.Lambda<Func<T, TKey>>(memberAccessExpr, new[] { recordArgExpr }).Compile();
            return _keyGetter;
        }

        public T Deserialize(BaseFileReader<T> reader, RecordReader recordReader, TKey valueOfKey)
        {
            if (_keySetter == null)
                _keySetter = GenerateKeySetter();

            var instance = CreateInstance();
            _keySetter(instance, valueOfKey);
            Deserialize(reader, recordReader);
            return instance;
        }

        public Action<T, TKey> GenerateKeySetter()
        {
            ExtendedMemberInfo keyMemberInfo = null;

            foreach (var memberInfo in Members)
            {
                if (IsMemberKey(memberInfo))
                {
                    keyMemberInfo = memberInfo;
                    break;
                }
            }

            if (keyMemberInfo == null)
                throw new InvalidOperationException("Unable to find a key column for type `" + typeof(T).Name + "`.");

            var newKeyArgExpr = Expression.Parameter(typeof(TKey), "key");
            var recordArgExpr = Expression.Parameter(typeof(T), "record");
            var memberAccessExpr = Expression.MakeMemberAccess(recordArgExpr, keyMemberInfo.MemberInfo);
            var assignmentExpr = Expression.Assign(memberAccessExpr, newKeyArgExpr);
            _keySetter = Expression.Lambda<Action<T, TKey>>(assignmentExpr, new[] { recordArgExpr, newKeyArgExpr }).Compile();
            return _keySetter;
        }
    }

    internal class CodeGenerator<T>
        where T : class, new()
    {
        private ParameterExpression _instance;

        public ExtendedMemberInfo[] Members { get; }

        private Action<BaseFileReader<T>, RecordReader, T> _deserializationMethod;
        private Func<T, T> _memberwiseClone;

        public bool IsIndexStreamed { get; set; }
        public int IndexColumn { get; set; }

        public CodeGenerator(ExtendedMemberInfo[] memberInfos)
        {
            _deserializationMethod = null;
            _memberwiseClone = null;
            
            _instance = Expression.Variable(typeof(T));

            Members = memberInfos;
        }

        public CodeGenerator(ParameterExpression instance, ExtendedMemberInfo[] memberInfos)
        {
            _instance = instance;

            Members = memberInfos;

            if (instance.Type != typeof(T))
                throw new InvalidOperationException("Type mismatch");
        }

        public virtual T CreateInstance()
        {
            return (T) typeof(T).CreateInstance();
        }

        public virtual T CreateInstance<TArg>(TArg arg1)
        {
            return (T) typeof(T).CreateInstance(arg1);
        }

        public virtual T CreateInstance<T1, T2>(T1 arg1, T2 arg2)
        {
            return (T) typeof(T).CreateInstance(arg1, arg2);
        }

        /// <summary>
        /// Given the provided <see cref="FileReader"/>, deserializes the record into a structure.
        /// </summary>
        /// <param name="fileReader"></param>
        /// <returns></returns>
        public virtual T Deserialize(BaseFileReader<T> fileReader, RecordReader recordReader)
        {
            if (_deserializationMethod == null)
                _deserializationMethod = GenerateDeserializationMethod();

            var instanceOfT = Activator.CreateInstance<T>();
            _deserializationMethod(fileReader, recordReader, instanceOfT);
            return instanceOfT;
        }

        protected T Deserialize(BaseFileReader<T> fileReader, RecordReader recordReader, T instance)
        {
            if (_deserializationMethod == null)
                _deserializationMethod = GenerateDeserializationMethod();

            _deserializationMethod(fileReader, recordReader, instance);
            return instance;
        }

        /// <summary>
        /// Produces a shallow copy of the provided object.
        /// </summary>
        /// <remarks>Will produce deep copies in the future.</remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        public T Clone(T input)
        {
            if (_memberwiseClone == null)
                _memberwiseClone = GenerateCloningMethod();

            return _memberwiseClone(input);
        }

        public Func<T, T> GenerateCloningMethod()
        {
            if (Members == null)
                throw new InvalidOperationException("Missing member informations in CodeGenerator<T>");

            var body = new List<Expression>();

            var inputNode = Expression.Parameter(typeof(T).MakeByRefType());
            var outputNode = Expression.Variable(typeof(T));
            var newNode = CreateTypeInitializer();

            body.Add(Expression.Assign(outputNode, newNode));

            foreach (var memberInfo in Members)
            {
                //! TODO: Handle deep copy
                var oldMember = memberInfo.MakeMemberAccess(inputNode);
                var newMember = memberInfo.MakeMemberAccess(outputNode);
                body.Add(Expression.Assign(newMember.Expression, oldMember.Expression));
            }

            body.Add(outputNode);
            var block = Expression.Block(new[] { outputNode }, body);
            var lmbda = Expression.Lambda<Func<T, T>>(block, inputNode);
            _memberwiseClone = lmbda.Compile();
            return _memberwiseClone;
        }

        /// <summary>
        /// Generates the deserialization method.
        /// </summary>
        /// <returns></returns>
        public Action<BaseFileReader<T>, RecordReader, T> GenerateDeserializationMethod()
        {
            if (Members == null)
                throw new InvalidOperationException("Missing member informations in CodeGenerator<T>");

            var binaryReader = Expression.Parameter(typeof(BaseFileReader<T>));
            var recordReader = Expression.Parameter(typeof(RecordReader));

            var bodyBlock = new List<Expression>();

            foreach (var memberInfo in Members)
            {
                if (memberInfo.MemberIndex == IndexColumn)
                    if (!IsIndexStreamed)
                        continue;

                var memberAccess = memberInfo.MakeMemberAccess(_instance);
                InsertMemberAssignment(bodyBlock, binaryReader, recordReader, memberAccess);
            }

            var expressionBody = Expression.Block(bodyBlock);
            var expressionLambda = Expression.Lambda<Action<BaseFileReader<T>, RecordReader, T>>(expressionBody, binaryReader, recordReader, _instance);

            var stringView = new ExpressionStringBuilder();
            stringView.Visit(expressionLambda);
            var resultStringView = stringView.ToString();
            return expressionLambda.Compile();
        }

        private void InsertMemberAssignment(List<Expression> bodyBlock, Expression binaryReaderInstance, Expression recordReaderInstance, ExtendedMemberExpression memberAccess)
        {
            switch (memberAccess.MemberInfo.CompressionType)
            {
                case MemberCompressionType.None:
                    InsertSimpleMemberAssignment(bodyBlock, binaryReaderInstance, recordReaderInstance, memberAccess);
                    break;
                case MemberCompressionType.Bitpacked:
                    InsertBitpackedMemberAssignment(bodyBlock, binaryReaderInstance, recordReaderInstance, memberAccess);
                    break;
                case MemberCompressionType.BitpackedPalletData:
                case MemberCompressionType.BitpackedPalletArrayData:
                    InsertPalletMemberAssignment(bodyBlock, binaryReaderInstance, recordReaderInstance, memberAccess);
                    break;
                case MemberCompressionType.CommonData:
                    InsertCommonMemberAssignment(bodyBlock, binaryReaderInstance, recordReaderInstance, memberAccess);
                    break;
                case MemberCompressionType.RelationshipData:
                    InsertRelationshipMemberAssignment(bodyBlock, binaryReaderInstance, recordReaderInstance, memberAccess);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void InsertRelationshipMemberAssignment(List<Expression> bodyBlock, Expression binaryReaderInstance, Expression recordReaderInstance, ExtendedMemberExpression memberAccess)
        {
            var commonReader = GenerateForeignKeyReader(binaryReaderInstance, recordReaderInstance, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, commonReader));
        }

        private void InsertCommonMemberAssignment(List<Expression> bodyBlock, Expression binaryReaderInstance, Expression recordReaderInstance, ExtendedMemberExpression memberAccess)
        {
            var commonReader = GenerateCommonReader(binaryReaderInstance, recordReaderInstance, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, commonReader));
        }

        private void InsertPalletMemberAssignment(List<Expression> bodyBlock, Expression binaryReaderInstance, Expression recordReaderInstance, ExtendedMemberExpression memberAccess)
        {
            if (memberAccess.MemberInfo.Type.IsArray == (memberAccess.MemberInfo.CompressionType == MemberCompressionType.BitpackedPalletData))
                throw new InvalidOperationException();
            
            if (memberAccess.MemberInfo.BitSize == 0)
                throw new InvalidOperationException();

            var palletReader = GeneratePalletReader(binaryReaderInstance, recordReaderInstance, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, palletReader));
        }

        private void InsertBitpackedMemberAssignment(List<Expression> bodyBlock, Expression binaryReaderInstance, Expression recordReaderInstance, ExtendedMemberExpression memberAccess)
        {
            if (memberAccess.MemberInfo.BitSize == 0)
                throw new InvalidOperationException();

            if (memberAccess.MemberInfo.Type.IsArray)
                throw new InvalidOperationException();

            var binaryReader = GenerateBinaryReader(binaryReaderInstance, recordReaderInstance, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, binaryReader));
        }

        private void InsertSimpleMemberAssignment(List<Expression> bodyBlock, Expression binaryReaderInstance, Expression recordReaderInstance, ExtendedMemberExpression memberAccess)
        {
            var binaryReader = GenerateBinaryReader(binaryReaderInstance, recordReaderInstance, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, binaryReader));
        }

        protected virtual Expression GenerateForeignKeyReader(Expression binaryReaderInstance, Expression recordReaderInstance, ExtendedMemberInfo memberInfo)
        {
            // TODO Fix this
            var methodInfo = binaryReaderInstance.Type.GetMethod("ReadForeignKeyMember").MakeGenericMethod(memberInfo.Type);
            return Expression.Call(binaryReaderInstance, methodInfo, Expression.Constant(memberInfo.MemberIndex), recordReaderInstance, _instance);
        }

        protected virtual Expression GenerateCommonReader(Expression binaryReaderInstance, Expression recordReaderInstance, ExtendedMemberInfo memberInfo)
        {
            // TODO Fix this
            var methodInfo = binaryReaderInstance.Type.GetMethod("ReadCommonMember").MakeGenericMethod(memberInfo.Type);
            return Expression.Call(binaryReaderInstance, methodInfo, Expression.Constant(memberInfo.MemberIndex), recordReaderInstance, _instance);
        }

        protected virtual Expression GeneratePalletReader(Expression binaryReaderInstance, Expression recordReaderInstance, ExtendedMemberInfo memberInfo)
        {
            // TODO Fix this
            MethodInfo methodInfo;
            if (memberInfo.Type.IsArray)
                methodInfo = binaryReaderInstance.Type.GetMethod("ReadPalletArrayMember").MakeGenericMethod(memberInfo.Type);
            else
                methodInfo = binaryReaderInstance.Type.GetMethod("ReadPalletMember").MakeGenericMethod(memberInfo.Type);

            return Expression.Call(binaryReaderInstance, methodInfo, Expression.Constant(memberInfo.MemberIndex), recordReaderInstance, _instance);
        }

        private Expression GenerateBinaryReader(Expression binaryReader, Expression recordReader, ExtendedMemberInfo memberInfo)
        {
            var elementType = memberInfo.Type.IsArray ? memberInfo.Type.GetElementType() : memberInfo.Type;
            var elementCode = Type.GetTypeCode(elementType);

            if (memberInfo.BitSize != 0 && memberInfo.OffsetInRecord != 0)
            {
                if (memberInfo.Type.IsArray)
                {
                    var methodInfo = elementCode == TypeCode.String ? _RecordReader.ReadPackedStrings : _RecordReader.ReadPackedArray;
                    return Expression.Call(recordReader, methodInfo, Expression.Constant(memberInfo.Cardinality), Expression.Constant(memberInfo.OffsetInRecord), Expression.Constant(memberInfo.BitSize));
                }
                else
                {
                    var methodInfo = _RecordReader.PackedReaders[elementCode];
                    return Expression.Call(recordReader, methodInfo, Expression.Constant(memberInfo.OffsetInRecord), Expression.Constant(memberInfo.BitSize));
                }
            }
            else
            {
                if (memberInfo.Type.IsArray)
                {
                    var methodInfo = elementCode == TypeCode.String ? _RecordReader.ReadStrings : _RecordReader.ReadArray;
                    return Expression.Call(recordReader, methodInfo, Expression.Constant(memberInfo.Cardinality));
                }
                else
                {
                    var methodInfo = _RecordReader.Readers[elementCode];
                    return Expression.Call(recordReader, methodInfo);
                }
            }
        }

        private Expression CreateTypeInitializer() => Expression.New(_instance.Type);
        
        private Expression CreateTypeInitializer(params ParameterExpression[] arguments)
        {
            // If a constructor is found with the provided parameters, use it.
            // Otherwise, well, fuck.
            var constructorInfo = _instance.Type.GetConstructor(arguments.Select(argument => argument.Type).ToArray());
            if (constructorInfo != null)
                return Expression.New(constructorInfo, arguments);
            
            // Use default parameterless constructor.
            return Expression.New(_instance.Type);
        }
    }
}
