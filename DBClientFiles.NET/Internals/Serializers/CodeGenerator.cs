using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Internals.Serializers
{
    internal class CodeGenerator<T, TKey> : CodeGenerator<T>
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
        public TKey ExtractKey(ref T instance)
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
        public void InsertKey(ref T instance, TKey key)
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
        public virtual bool IsMemberKey(ExtendedMemberInfo memberInfo) => memberInfo.IsDefined(typeof(IndexAttribute), false);

        /// <summary>
        /// Generates a key extractor method.
        /// </summary>
        /// <returns></returns>
        public Func<T, TKey> GenerateKeyGetter()
        {
            ExtendedMemberInfo keyMemberInfo = null;

            foreach (var memberInfo in Members)
            {
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
                throw new InvalidOperationException("Unable to find a key column for type `" + typeof(T).Name + "`.");

            var recordArgExpr = Expression.Parameter(typeof(T), "record");
            var memberAccessExpr = Expression.MakeMemberAccess(recordArgExpr, keyMemberInfo.MemberInfo);
            _keyGetter = Expression.Lambda<Func<T, TKey>>(memberAccessExpr, new[] { recordArgExpr }).Compile();
            return _keyGetter;
        }

        public Action<T, TKey> GenerateKeySetter()
        {
            ExtendedMemberInfo keyMemberInfo = null;

            foreach (var memberInfo in Members)
            {
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
    {
        private ParameterExpression _instance;

        public ExtendedMemberInfo[] Members { get; }

        private Func<FileReader, T> _deserializationMethod;
        private Func<T, T> _memberwiseClone;

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

        /// <summary>
        /// Given the provided <see cref="FileReader"/>, deserializes the record into a structure.
        /// </summary>
        /// <param name="fileReader"></param>
        /// <returns></returns>
        public T Deserialize(FileReader fileReader)
        {
            if (_deserializationMethod == null)
                _deserializationMethod = GenerateDeserializationMethod();

            return _deserializationMethod(fileReader);
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
        public Func<FileReader, T> GenerateDeserializationMethod()
        {
            if (Members == null)
                throw new InvalidOperationException("Missing member informations in CodeGenerator<T>");

            var binaryReader = Expression.Parameter(typeof(FileReader));

            var bodyBlock = new List<Expression>() {
                Expression.Assign(_instance, CreateTypeInitializer())
            };

            foreach (var memberInfo in Members)
            {
                var memberAccess = memberInfo.MakeMemberAccess(_instance);
                InsertMemberAssignment(bodyBlock, binaryReader, memberAccess);
            }

            bodyBlock.Add(_instance);

            var expressionBody = Expression.Block(new[] { _instance }, bodyBlock);
            var expressionLambda = Expression.Lambda<Func<FileReader, T>>(expressionBody, binaryReader);

            var stringView = new ExpressionStringBuilder();
            stringView.Visit(expressionLambda);
            var resultStringView = stringView.ToString();
            return expressionLambda.Compile();
        }

        private void InsertMemberAssignment(List<Expression> bodyBlock, Expression binaryReaderInstance, ExtendedMemberExpression memberAccess)
        {
            switch (memberAccess.MemberInfo.CompressionType)
            {
                case MemberCompressionType.None:
                    InsertSimpleMemberAssignment(bodyBlock, binaryReaderInstance, memberAccess);
                    break;
                case MemberCompressionType.Bitpacked:
                    InsertBitpackedMemberAssignment(bodyBlock, binaryReaderInstance, memberAccess);
                    break;
                case MemberCompressionType.BitpackedPalletData:
                case MemberCompressionType.BitpackedPalletArrayData:
                    InsertPalletMemberAssignment(bodyBlock, binaryReaderInstance, memberAccess);
                    break;
                case MemberCompressionType.CommonData:
                    InsertCommonMemberAssignment(bodyBlock, binaryReaderInstance, memberAccess);
                    break;
                case MemberCompressionType.RelationshipData:
                    InsertRelationshipMemberAssignment(bodyBlock, binaryReaderInstance, memberAccess);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void InsertRelationshipMemberAssignment(List<Expression> bodyBlock, Expression binaryReaderInstance, ExtendedMemberExpression memberAccess)
        {
            var commonReader = GenerateForeignKeyReader(binaryReaderInstance, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, commonReader));
        }

        private void InsertCommonMemberAssignment(List<Expression> bodyBlock, Expression binaryReaderInstance, ExtendedMemberExpression memberAccess)
        {
            var commonReader = GenerateCommonReader(binaryReaderInstance, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, commonReader));
        }

        private void InsertPalletMemberAssignment(List<Expression> bodyBlock, Expression binaryReaderInstance, ExtendedMemberExpression memberAccess)
        {
            if (memberAccess.MemberInfo.Type.IsArray == (memberAccess.MemberInfo.CompressionType == MemberCompressionType.BitpackedPalletData))
                throw new InvalidOperationException();
            
            if (memberAccess.MemberInfo.BitSize == 0)
                throw new InvalidOperationException();

            var palletReader = GeneratePalletReader(binaryReaderInstance, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, palletReader));
        }

        private void InsertBitpackedMemberAssignment(List<Expression> bodyBlock, Expression binaryReaderInstance, ExtendedMemberExpression memberAccess)
        {
            if (memberAccess.MemberInfo.BitSize == 0)
                throw new InvalidOperationException();

            if (memberAccess.MemberInfo.Type.IsArray)
                throw new InvalidOperationException();

            var binaryReader = GenerateBinaryReader(binaryReaderInstance, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, binaryReader));
        }

        private void InsertSimpleMemberAssignment(List<Expression> bodyBlock, Expression binaryReaderInstance, ExtendedMemberExpression memberAccess)
        {
            var binaryReader = GenerateBinaryReader(binaryReaderInstance, memberAccess.MemberInfo);
            if (memberAccess.MemberInfo.Type.IsArray)
            {
                bodyBlock.Add(Expression.Assign(
                    memberAccess.Expression,
                    Expression.NewArrayBounds(memberAccess.MemberInfo.Type.GetElementType(), Expression.Constant(memberAccess.MemberInfo.ArraySize))));
                for (var i = 0; i < memberAccess.MemberInfo.ArraySize; ++i)
                {
                    var itemAccess = Expression.ArrayAccess(memberAccess.Expression, Expression.Constant(i));

                    bodyBlock.Add(Expression.Assign(itemAccess, binaryReader));
                }
            }
            else
                bodyBlock.Add(Expression.Assign(memberAccess.Expression, binaryReader));
        }

        protected virtual Expression GenerateForeignKeyReader(Expression binaryReaderInstance, ExtendedMemberInfo memberInfo)
        {
            return Expression.Call(binaryReaderInstance, _FileReader.ReadForeignKeyMember, Expression.Constant(memberInfo.MemberIndex));
        }

        protected virtual Expression GenerateCommonReader(Expression binaryReaderInstance, ExtendedMemberInfo memberInfo)
        {
            return Expression.Call(binaryReaderInstance, _FileReader.ReadCommonMember, Expression.Constant(memberInfo.MemberIndex));
        }

        protected virtual Expression GeneratePalletReader(Expression binaryReaderInstance, ExtendedMemberInfo memberInfo)
        {
            if (memberInfo.Type.IsArray)
                return Expression.Call(binaryReaderInstance, _FileReader.ReadPalletArrayMember, Expression.Constant(memberInfo.MemberIndex));

            return Expression.Call(binaryReaderInstance, _FileReader.ReadPalletMember, Expression.Constant(memberInfo.MemberIndex));
        }

        protected virtual Expression GenerateBinaryReader(Expression binaryReaderInstance, ExtendedMemberInfo memberInfo)
        {
            var memberType = memberInfo.Type;
            if (memberType.IsArray)
                memberType = memberType.GetElementType();

            if (memberInfo.BitSize != 0)
            {
                var methodCall = Expression.Call(binaryReaderInstance, _FileReader.ReadBits, Expression.Constant(memberInfo.BitSize));
                return Expression.Convert(methodCall, memberType);
            }

            return GenerateBinaryReader(binaryReaderInstance, memberType);
        }

        protected virtual Expression GenerateBinaryReader(Expression binaryReaderInstance, Type memberType)
        {
            var memberCode = Type.GetTypeCode(memberType);

            switch (memberCode)
            {
                case TypeCode.UInt64: return Expression.Call(binaryReaderInstance, _FileReader.ReadUInt64);
                case TypeCode.UInt32: return Expression.Call(binaryReaderInstance, _FileReader.ReadUInt32);
                case TypeCode.UInt16: return Expression.Call(binaryReaderInstance, _FileReader.ReadUInt16);
                case TypeCode.Byte:   return Expression.Call(binaryReaderInstance, _FileReader.ReadByte);
                case TypeCode.Int64:  return Expression.Call(binaryReaderInstance, _FileReader.ReadInt64);
                case TypeCode.Int32:  return Expression.Call(binaryReaderInstance, _FileReader.ReadInt32);
                case TypeCode.Int16:  return Expression.Call(binaryReaderInstance, _FileReader.ReadInt16);
                case TypeCode.SByte:  return Expression.Call(binaryReaderInstance, _FileReader.ReadSByte);
                case TypeCode.String: return Expression.Call(binaryReaderInstance, _FileReader.ReadString);
                case TypeCode.Single: return Expression.Call(binaryReaderInstance, _FileReader.ReadSingle);
            }

            throw new NotImplementedException();
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
