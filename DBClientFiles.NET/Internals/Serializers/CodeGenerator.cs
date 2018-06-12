using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Internals.Versions;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals.Binding;

namespace DBClientFiles.NET.Internals.Serializers
{
    internal class CodeGenerator<T, TKey> : CodeGenerator<T>
        where T : class, new()
        where TKey : struct
    {
        private Func<T, TKey> _keyGetter;
        private Action<T, TKey> _keySetter;

        #region Life and death
        public CodeGenerator(BaseFileReader<T> reader) : base(reader)
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
        /// <param name="instance">The target instance of <see cref="T"/></param>
        /// <param name="key">The value to assign to the member representating a key.</param>
        public void InsertKey(T instance, TKey key)
        {
            if (_keySetter == null)
                _keySetter = GenerateKeySetter();

            _keySetter(instance, key);
        }

        /// <summary>
        /// Generates a key extractor method.
        /// </summary>
        /// <returns></returns>
        public Func<T, TKey> GenerateKeyGetter()
        {
            var keyMemberInfo = Reader.MemberStore.IndexMember;
#if DEBUG
            if (keyMemberInfo == null)
                throw new InvalidOperationException("Unable to find a key column for type `" + typeof(T).Name + "`.");
#endif
            var recordArgExpr = Expression.Parameter(typeof(T), "record");
            var memberAccessExpr = Expression.MakeMemberAccess(recordArgExpr, keyMemberInfo.MemberInfo);
            var expressionLambda = Expression.Lambda<Func<T, TKey>>(memberAccessExpr, recordArgExpr);
            _keyGetter = expressionLambda.Compile();

#if DEBUG
            var resultStringView = ExpressionStringBuilder.ExpressionToString(expressionLambda);
            Console.WriteLine($"Key getter for {typeof(T).Name}");
            Console.WriteLine(resultStringView);
#endif

            return _keyGetter;
        }

        public T Deserialize(BaseFileReader<T> reader, RecordReader recordReader, TKey recordKey)
        {
            if (_keySetter == null)
                _keySetter = GenerateKeySetter();

            var instance = CreateInstance();
            _keySetter(instance, recordKey);
            return Deserialize(instance, reader, recordReader);
        }

        public Action<T, TKey> GenerateKeySetter()
        {
            var keyMemberInfo = Reader.MemberStore.IndexMember;
#if DEBUG
            if (keyMemberInfo == null)
                throw new InvalidOperationException("Unable to find a key column for type `" + typeof(T).Name + "`.");
#endif
            var newKeyArgExpr = Expression.Parameter(typeof(TKey), "key");
            var recordArgExpr = Expression.Parameter(typeof(T), "record");
            var memberAccessExpr = Expression.MakeMemberAccess(recordArgExpr, keyMemberInfo.MemberInfo);
            var assignmentExpr = Expression.Assign(memberAccessExpr, newKeyArgExpr);
            var expressionLambda = Expression.Lambda<Action<T, TKey>>(assignmentExpr, recordArgExpr, newKeyArgExpr);
            _keySetter = expressionLambda.Compile();

#if DEBUG
            var resultStringView = ExpressionStringBuilder.ExpressionToString(expressionLambda);
            Console.WriteLine($"Key setter for {typeof(T).Name}");
            Console.WriteLine(resultStringView);
#endif

            return _keySetter;
        }
    }

    internal class CodeGenerator<T>
        where T : class, new()
    {
        private readonly ParameterExpression _instance;

        public BaseFileReader<T> Reader { get; }

        private Action<BaseFileReader<T>, RecordReader, T> _deserializationMethod;
        private Func<T, T> _memberwiseClone;

        public CodeGenerator(BaseFileReader<T> reader)
        {
            _deserializationMethod = null;
            _memberwiseClone = null;
            
            _instance = Expression.Variable(typeof(T), "instance");

            Reader = reader;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual T CreateInstance()
        {
            return New<T>.Instance();
        }

        /// <summary>
        /// Given the provided <see cref="FileReader"/>, deserializes the record into a structure.
        /// </summary>
        /// <param name="fileReader"></param>
        /// <param name="recordReader"></param>
        /// <returns></returns>
        public virtual T Deserialize(BaseFileReader<T> fileReader, RecordReader recordReader)
        {
            if (_deserializationMethod == null)
                _deserializationMethod = GenerateDeserializationMethod();

            var instanceOfT = CreateInstance();
            _deserializationMethod(fileReader, recordReader, instanceOfT);
            return instanceOfT;
        }

        protected T Deserialize(T instance, BaseFileReader<T> fileReader, RecordReader recordReader)
        {
            if (_deserializationMethod == null)
                _deserializationMethod = GenerateDeserializationMethod();
            
            _deserializationMethod(fileReader, recordReader, instance);
            return instance;
        }

#region Hack for StorageDictionary
        public TKey ExtractKey<TKey>(T instance)
            where TKey : struct
        {
            if (!(this is CodeGenerator<T, TKey> codeGen))
                throw new InvalidOperationException();

            return codeGen.ExtractKey(instance);
        }
#endregion

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
            if (Reader.MemberStore.Members.Count == 0)
                throw new InvalidOperationException("Missing member informations in CodeGenerator<T>");

            var body = new List<Expression>();

            var inputNode = Expression.Parameter(typeof(T), "source");
            var outputNode = Expression.Variable(typeof(T), "destination");
            var newNode = CreateTypeInitializer();

            body.Add(Expression.Assign(outputNode, newNode));
            
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Reader.MemberStore.Members.Count; ++i)
            {
                var memberInfo = Reader.MemberStore.Members[i];

                var oldMember = memberInfo.MakeMemberAccess(inputNode);
                var newMember = memberInfo.MakeMemberAccess(outputNode);

                CloneMember(body, ref oldMember, ref newMember);
            }

            body.Add(outputNode);
            var block = Expression.Block(new[] { outputNode }, body);
            var lmbda = Expression.Lambda<Func<T, T>>(block, inputNode);
#if DEBUG
            var resultStringView = ExpressionStringBuilder.ExpressionToString(lmbda);
            Console.WriteLine($"Cloner for {typeof(T).Name}");
            Console.WriteLine(resultStringView);
#endif

            _memberwiseClone = lmbda.Compile();
            return _memberwiseClone;
        }

        private static void CloneMember(List<Expression> bodyBlock, ref ExtendedMemberExpression inputNode, ref ExtendedMemberExpression outputNode)
        {
            var memberInfo = inputNode.MemberInfo;
            if (memberInfo.Children.Count != 0)
            {
                if (memberInfo.Type.IsArray)
                    throw new InvalidStructureException("Structures may not contain substructure arrays!");

                if (!memberInfo.Type.IsValueType)
                    bodyBlock.Add(Expression.Assign(outputNode.Expression, CreateTypeInitializer(memberInfo.Type)));

                foreach (var childInfo in memberInfo.Children)
                {
                    var inputChildExpression = childInfo.MakeMemberAccess(inputNode.Expression);
                    var outputChildExpression = childInfo.MakeMemberAccess(outputNode.Expression);
                    CloneMember(bodyBlock, ref inputChildExpression, ref outputChildExpression);
                }
            }
            else if (memberInfo.Type.IsArray)
            {
                bodyBlock.Add(Expression.Assign(outputNode.Expression,
                    Expression.NewArrayBounds(memberInfo.Type.GetElementType(),
                        Expression.Constant(memberInfo.Cardinality))));

                for (var i = 0; i < memberInfo.Cardinality; ++i)
                {
                    var itr = Expression.Constant(i);
                    var inputArrayElement = Expression.ArrayIndex(inputNode.Expression, itr);
                    var outputArrayElement = Expression.ArrayIndex(outputNode.Expression, itr);
                    bodyBlock.Add(Expression.Assign(outputArrayElement, inputArrayElement));
                }
            }
            else
            {
                bodyBlock.Add(Expression.Assign(outputNode.Expression, inputNode.Expression));
            }
        }

        /// <summary>
        /// Generates the deserialization method.
        /// </summary>
        /// <returns></returns>
        public Action<BaseFileReader<T>, RecordReader, T> GenerateDeserializationMethod()
        {
#if DEBUG
            if (Reader.MemberStore.Members.Count == 0)
                throw new InvalidOperationException("Missing member informations in CodeGenerator<T>");
#endif

            var binaryReader = Expression.Parameter(typeof(BaseFileReader<T>), "fileReader");
            var recordReader = Expression.Parameter(typeof(RecordReader), "recordReader");

            var bodyBlock = new List<Expression>();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < Reader.MemberStore.Members.Count; index++)
            {
                var memberInfo = Reader.MemberStore.Members[index];

                if (memberInfo.MemberInfo.IsDefined(typeof(IgnoreAttribute), false))
                    continue;
                
                if (memberInfo.MappedTo == null)
                {
                    if (memberInfo.Children.Count == 0)
                        continue;
                }
                
                var memberAccess = memberInfo.MakeMemberAccess(_instance);
                BindMember(bodyBlock, binaryReader, recordReader, ref memberAccess);
            }

            if (bodyBlock.Count == 0)
                throw new InvalidOperationException($"Unable to find any member to deserialize in {typeof(T).FullName}");

            var expressionBody = Expression.Block(bodyBlock);
            var expressionLambda = Expression.Lambda<Action<BaseFileReader<T>, RecordReader, T>>(expressionBody, binaryReader, recordReader, _instance);

#if DEBUG
            var resultStringView = ExpressionStringBuilder.ExpressionToString(expressionLambda);
            Console.WriteLine($"Deserializer for {typeof(T).Name}");
            Console.WriteLine(resultStringView);
#endif

            return expressionLambda.Compile();
        }

        private void BindMember(List<Expression> bodyBlock, Expression binaryReader, Expression recordReader, ref ExtendedMemberExpression memberAccess)
        {
            if (memberAccess.MemberInfo.Children.Count != 0)
            {
                //! TODO Implement (weird things like C3Vector[2])
                if (memberAccess.MemberInfo.Type.IsArray)
                    throw new InvalidStructureException("Structures may not contain substructure arrays!");

                if (!memberAccess.MemberInfo.Type.IsValueType)
                    bodyBlock.Add(Expression.Assign(memberAccess.Expression, CreateTypeInitializer(memberAccess.MemberInfo.Type)));

                foreach (var childInfo in memberAccess.MemberInfo.Children)
                {
                    var childExpression = childInfo.MakeMemberAccess(memberAccess.Expression);
                    BindMember(bodyBlock, binaryReader, recordReader, ref childExpression);
                }
            }
            else
                InsertMemberAssignment(bodyBlock, binaryReader, recordReader, ref memberAccess);
        }

        private void InsertMemberAssignment(List<Expression> bodyBlock, Expression binaryReader, Expression recordReader, ref ExtendedMemberExpression memberAccess)
        {
            switch (memberAccess.MemberInfo.MappedTo.CompressionType)
            {
                case MemberCompressionType.None:
                    InsertSimpleMemberAssignment(bodyBlock, recordReader, ref memberAccess);
                    break;
                case MemberCompressionType.SignedImmediate:
                case MemberCompressionType.Immediate:
                    InsertBitpackedMemberAssignment(bodyBlock, recordReader, ref memberAccess);
                    break;
                case MemberCompressionType.BitpackedPalletData:
                case MemberCompressionType.BitpackedPalletArrayData:
                    InsertPalletMemberAssignment(bodyBlock, binaryReader, recordReader, ref memberAccess);
                    break;
                case MemberCompressionType.CommonData:
                    InsertCommonMemberAssignment(bodyBlock, binaryReader, recordReader, ref memberAccess);
                    break;
                case MemberCompressionType.RelationshipData:
                    if (!_instance.Type.IsDefined(typeof(IgnoreRelationshipDataAttribute), false))
                        InsertRelationshipMemberAssignment(bodyBlock, binaryReader, recordReader, ref memberAccess);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(memberAccess.MemberInfo.MappedTo.CompressionType));
            }
        }

        private void InsertRelationshipMemberAssignment(List<Expression> bodyBlock, Expression binaryReader, Expression recordReader, ref ExtendedMemberExpression memberAccess)
        {
            var commonReader = GenerateForeignKeyReader(binaryReader, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, commonReader));
        }

        private void InsertCommonMemberAssignment(List<Expression> bodyBlock, Expression binaryReader, Expression recordReader, ref ExtendedMemberExpression memberAccess)
        {
            var commonReader = GenerateCommonReader(binaryReader, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, commonReader));
        }

        private void InsertPalletMemberAssignment(List<Expression> bodyBlock, Expression binaryReader, Expression recordReader, ref ExtendedMemberExpression memberAccess)
        {
            if (memberAccess.MemberInfo.Type.IsArray &&
                (memberAccess.MemberInfo.MappedTo.CompressionType == MemberCompressionType.BitpackedPalletData))
                throw new InvalidMemberException(memberAccess.MemberInfo.MemberInfo,
                    "Member {0} of type {1} is declared as an array but file metadata tells us it's not!");
            
            if (memberAccess.MemberInfo.MappedTo.BitSize == 0)
                throw new InvalidOperationException();

            var palletReader = GeneratePalletReader(binaryReader, recordReader, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, palletReader));
        }

        private void InsertBitpackedMemberAssignment(List<Expression> bodyBlock, Expression recordReader, ref ExtendedMemberExpression memberAccess)
        {
            if (memberAccess.MemberInfo.MappedTo.BitSize == 0)
                throw new InvalidOperationException();

            if (memberAccess.MemberInfo.Type.IsArray)
                throw new InvalidMemberException(memberAccess.MemberInfo.MemberInfo,
                    "Member {0} of type {1} is declared as an array but file metadata tells us it's not!");

            var binaryReader = GenerateBinaryReader(recordReader, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, binaryReader));
        }
        
        private void InsertSimpleMemberAssignment(List<Expression> bodyBlock, Expression recordReader, ref ExtendedMemberExpression memberAccess)
        {
            var binaryReader = GenerateBinaryReader(recordReader, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, binaryReader));
        }

        protected virtual Expression GenerateForeignKeyReader(Expression binaryReader, ExtendedMemberInfo memberInfo)
        {
            var methodInfo = binaryReader.Type.GetMethod("ReadForeignKeyMember").MakeGenericMethod(memberInfo.Type);
            return Expression.Call(binaryReader, methodInfo);
        }

        protected virtual Expression GenerateCommonReader(Expression binaryReader, ExtendedMemberInfo memberInfo)
        {
            var methodInfo = binaryReader.Type.GetMethod("ReadCommonMember").MakeGenericMethod(memberInfo.Type);
            return Expression.Call(binaryReader, methodInfo, Expression.Constant(memberInfo.MappedTo.Index), _instance);
        }

        protected virtual Expression GeneratePalletReader(Expression binaryReader, Expression recordReader, ExtendedMemberInfo memberInfo)
        {
            MethodInfo methodInfo;
            if (memberInfo.Type.IsArray)
                methodInfo = binaryReader.Type.GetMethod("ReadPalletArrayMember").MakeGenericMethod(memberInfo.Type.GetElementType());
            else
                methodInfo = binaryReader.Type.GetMethod("ReadPalletMember").MakeGenericMethod(memberInfo.Type);

            return Expression.Call(binaryReader, methodInfo, Expression.Constant(memberInfo.MappedTo.Index), recordReader, _instance);
        }

        private Expression GenerateBinaryReader(Expression recordReader, ExtendedMemberInfo memberInfo)
        {
            var elementType = memberInfo.Type.IsArray ? memberInfo.Type.GetElementType() : memberInfo.Type;
            var elementCode = Type.GetTypeCode(elementType);

            if (!Reader.Options.OverrideSignedChecks && memberInfo.MappedTo.IsSigned.HasValue)
            {
                if (elementType.IsSigned() != memberInfo.MappedTo.IsSigned)
                    throw new InvalidMemberException(memberInfo.MemberInfo,
                        "Member {0} of type {1} is declared as {2} but file metadata says otherwise. Is your structure correct?",
                        elementType.IsSigned() ? "signed" : "unsigned");
            }

            if (memberInfo.MappedTo.BitSize != 0)
            {
                if (memberInfo.Type.IsArray)
                {
                    var stringMethodInfo = elementCode == TypeCode.String
                        ? _RecordReader.ReadPackedStrings
                        : _RecordReader.ReadPackedArray.MakeGenericMethod(elementType);
                    return Expression.Call(recordReader, stringMethodInfo, Expression.Constant(memberInfo.Cardinality),
                        Expression.Constant(memberInfo.MappedTo.Offset), Expression.Constant(memberInfo.MappedTo.BitSize));
                }

                if (elementCode == TypeCode.Single)
                {
                    if ((memberInfo.MappedTo.Offset & 7) != 0)
                        throw new InvalidMemberException(memberInfo.MemberInfo,
                            "Member {0} of type {1} is declared as unaligned (spanning bits {2}-{3}) by file data. Floats have to be aligned. Is your structure correct?",
                            memberInfo.MappedTo.Offset,
                            memberInfo.MappedTo.Offset + memberInfo.MappedTo.BitSize);
                    
                    if (memberInfo.MappedTo.BitSize != 32)
                        throw new InvalidMemberException(memberInfo.MemberInfo,
                            "Member {0} of type {1} is declared as packed (Occupying {2} bits) by file data. Floats cannot be packed. Is your structure correct?",
                            memberInfo.MappedTo.BitSize);

                    return Expression.Call(recordReader, _RecordReader.ReadPackedSingle, Expression.Constant(memberInfo.MappedTo.Offset));
                }

                var methodInfo = _RecordReader.PackedReaders[elementCode];
                return Expression.Call(recordReader, methodInfo, Expression.Constant(memberInfo.MappedTo.Offset), Expression.Constant(memberInfo.MappedTo.BitSize));
            }

            if (memberInfo.Type.IsArray)
            {
                var methodInfo = elementCode == TypeCode.String
                    ? _RecordReader.ReadStrings
                    : _RecordReader.ReadArray.MakeGenericMethod(elementType);
                return Expression.Call(recordReader, methodInfo, Expression.Constant(memberInfo.Cardinality));
            }
            else
            {
                var methodInfo = _RecordReader.Readers[elementCode];
                return Expression.Call(recordReader, methodInfo);
            }
        }

        private Expression CreateTypeInitializer() => CreateTypeInitializer(_instance.Type);

        private Expression CreateTypeInitializer(params Expression[] arguments) => CreateTypeInitializer(_instance.Type, arguments);

        private static Expression CreateTypeInitializer(Type instanceType) => Expression.New(instanceType);
        
        private static Expression CreateTypeInitializer(Type instanceType, params Expression[] arguments)
        {
            // If a constructor is found with the provided parameters, use it.
            // Otherwise, well, fuck.
            var constructorInfo = instanceType.GetConstructor(arguments.Select(argument => argument.Type).ToArray());
            if (constructorInfo != null)
                return Expression.New(constructorInfo, arguments);

            if (instanceType.IsValueType)
                return null;

            // Use default parameterless constructor.
            return CreateTypeInitializer(instanceType);
        }
    }
}
