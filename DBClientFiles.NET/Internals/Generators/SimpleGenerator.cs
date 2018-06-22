using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals.Binding;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Generators
{
    internal class SimpleGenerator<TKey, T> : SimpleGenerator<T>, IGenerator<TKey, T>
    {
        private Func<T, TKey> _keyGetter;
        private Action<T, TKey> _keySetter;

        public SimpleGenerator(FileReader fileReader) : base(fileReader)
        {

        }

        public TKey ExtractKey(T instance)
        {
            if (_keyGetter == null)
                _keyGetter = GenerateKeyGetter();

            return _keyGetter(instance);
        }

        public void InsertKey(T instance, TKey keyValue)
        {
            if (_keySetter == null)
                _keySetter = GenerateKeySetter();

            _keySetter(instance, keyValue);
        }

        public void Deserialize(T instance, TKey forcedKeyValue, RecordReader recordReader)
        {
            InsertKey(instance, forcedKeyValue);
            Deserialize(instance, recordReader);
        }

        public T Deserialize(TKey forcedKeyValue, RecordReader recordReader)
        {
            var instance = New<T>.Instance();
            Deserialize(instance, forcedKeyValue, recordReader);
            return instance;
        }

        #region Method generators
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
        #endregion
    }

    internal class SimpleGenerator<T> : IGenerator<T>
    {
        private readonly ParameterExpression _instance;

        public StorageOptions Options => Reader.Options;
        public IFileHeader Header => Reader.Header;
        public FileReader Reader { get; }

        private Action<FileReader, RecordReader, T> _deserializationMethod;
        private Func<T, T> _memberwiseClone;

        public SimpleGenerator(FileReader fileReader)
        {
            _instance = Expression.Parameter(typeof(T));

            Reader = fileReader;
        }

        public T Deserialize(RecordReader recordReader)
        {
            var instance = New<T>.Instance();
            Deserialize(instance, recordReader);
            return instance;
        }

        public void Deserialize(T instance, RecordReader recordReader)
        {
            if (_deserializationMethod == null)
                _deserializationMethod = CreateDeserializer();

            _deserializationMethod(Reader, recordReader, instance);
        }

        public T Clone(T sourceInstance)
        {
            if (_memberwiseClone == null)
                _memberwiseClone = CreateCloner();

            return _memberwiseClone(sourceInstance);
        }

        #region Cloning
        public Func<T, T> CreateCloner()
        {
            if (Reader.MemberStore.Members.Count == 0)
                throw new InvalidOperationException("Missing member informations in CodeGenerator<T>");

            var body = new List<Expression>();

            var inputNode = Expression.Parameter(typeof(T), "source");
            var outputNode = Expression.Variable(typeof(T), "destination");
            var newNode = CreateTypeInitializer(typeof(T));

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
                var newArray = Expression.NewArrayBounds(memberInfo.Type.GetElementType(), Expression.Constant(memberInfo.Cardinality));
                bodyBlock.Add(Expression.Assign(outputNode.Expression, newArray));

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
        #endregion

        #region Deserialization

        /// <summary>
        /// Generates the deserialization method.
        /// </summary>
        /// <returns></returns>
        public Action<FileReader, RecordReader, T> CreateDeserializer()
        {
#if DEBUG
            if (Reader.MemberStore.Members.Count == 0)
                throw new InvalidOperationException("Missing member informations in CodeGenerator<T>");
#endif

            var binaryReader = Expression.Parameter(typeof(FileReader), "fileReader");
            var recordReader = Expression.Parameter(typeof(RecordReader), "recordReader");

            var bodyBlock = new List<Expression>();

            foreach (var memberInfo in Reader.MemberStore.Members)
            {
                if (memberInfo.MemberInfo.IsDefined(typeof(IgnoreAttribute), false))
                    continue;

                if (memberInfo.MappedTo == null && memberInfo.Children.Count == 0)
                    continue;

                var memberAccess = memberInfo.MakeMemberAccess(_instance);
                BindMember(bodyBlock, recordReader, ref memberAccess);
            }

            if (bodyBlock.Count == 0)
                throw new InvalidOperationException($"Unable to find any member to deserialize in {typeof(T).FullName}");

            var expressionBody = Expression.Block(bodyBlock);
            var expressionLambda = Expression.Lambda<Action<FileReader, RecordReader, T>>(expressionBody, binaryReader, recordReader, _instance);

#if DEBUG
            var resultStringView = ExpressionStringBuilder.ExpressionToString(expressionLambda);
            Console.WriteLine($"Deserializer for {typeof(T).Name}");
            Console.WriteLine(resultStringView);
#endif

            return expressionLambda.Compile();
        }

        private void BindMember(List<Expression> bodyBlock, Expression recordReader, ref ExtendedMemberExpression memberAccess)
        {
            if (memberAccess.MemberInfo.Children.Count != 0)
            {
                if (memberAccess.MemberInfo.Type.IsArray)
                    throw new InvalidStructureException("Structures may not contain substructure arrays!");

                // This is not exactly a needed check but it helps avoid an unneeded default(T) assignment.
                if (!memberAccess.MemberInfo.Type.IsValueType)
                    bodyBlock.Add(Expression.Assign(memberAccess.Expression, CreateTypeInitializer(memberAccess.MemberInfo.Type)));

                foreach (var childInfo in memberAccess.MemberInfo.Children)
                {
                    var childExpression = childInfo.MakeMemberAccess(memberAccess.Expression);
                    BindMember(bodyBlock, recordReader, ref childExpression);
                }
            }
            else
                InsertSimpleMemberAssignment(bodyBlock, recordReader, ref memberAccess);
        }

        private void InsertSimpleMemberAssignment(List<Expression> bodyBlock, Expression recordReader, ref ExtendedMemberExpression memberAccess)
        {
            var binaryReader = GenerateBinaryReader(recordReader, memberAccess.MemberInfo);
            bodyBlock.Add(Expression.Assign(memberAccess.Expression, binaryReader));
        }

        private Expression GenerateBinaryReader(Expression recordReader, ExtendedMemberInfo memberInfo)
        {
            var elementType = memberInfo.Type.IsArray ? memberInfo.Type.GetElementType() : memberInfo.Type;
            var elementCode = Type.GetTypeCode(elementType);

            if (!Options.IgnoreSignedChecks && memberInfo.MappedTo.IsSigned.HasValue)
            {
                if (elementType.IsSigned() != memberInfo.MappedTo.IsSigned)
                    throw new InvalidMemberException(memberInfo.MemberInfo,
                        "Member {0} of type {1} is declared as {2} but file metadata says otherwise. Is your structure correct?",
                        elementType.IsSigned() ? "signed" : "unsigned");
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

        #endregion

        private static Expression CreateTypeInitializer(Type instanceType) => Expression.New(instanceType);
        private static Expression CreateTypeInitializer(Type instanceType, params Expression[] arguments)
        {
            // If a constructor is found with the provided parameters, use it.
            // Otherwise, well, fuck.
            var constructorInfo = instanceType.GetConstructor(arguments.Select(argument => argument.Type).ToArray());
            if (constructorInfo != null)
                return Expression.New(constructorInfo, arguments);

            if (instanceType.IsValueType)
                return Expression.Default(instanceType);

            // Use default parameterless constructor.
            return CreateTypeInitializer(instanceType);
        }
    }
}
