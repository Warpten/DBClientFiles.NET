using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization.Runtime;
using DBClientFiles.NET.Parsing.Shared.Records;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace DBClientFiles.NET.Parsing.Versions.WDBC
{
    /// <summary>
    /// Generates a method capable of deserializing a given type at runtime from a WDBC stream.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class RuntimeDeserializer<T> : TypeDeserializerRuntimeMethod<RuntimeDeserializer<T>.MethodType, T>
    {
        public delegate void MethodType(Stream dataStream, in AlignedSequentialRecordReader recordReader, out T instance);

        private ParameterExpression DataStream { get; }
        private ParameterExpression RecordReader { get; }
        protected override ParameterExpression Instance { get; }

        private Lazy<MethodType> _methodInitializer;
        private MethodType Method => _methodInitializer.Value;

        public RuntimeDeserializer(TypeToken typeToken, TypeTokenType typeTokenType) : base(typeToken, typeTokenType)
        {
            DataStream = Expression.Parameter(typeof(Stream), "dataStream");
            RecordReader = Expression.Parameter(typeof(AlignedSequentialRecordReader).MakeByRefType(), "recordReader");
            Instance = Expression.Parameter(typeof(T).MakeByRefType(), "instance");

            _methodInitializer = new Lazy<MethodType>(() => Expression.Lambda<MethodType>(CreateBody(),
                DataStream,
                RecordReader,
                Instance
            ).Compile());
        }


        protected override Expression CreateArrayInitializer(MemberToken memberToken) => null;

        protected override Expression CreateInstanceInitializer(TypeToken typeToken)
        {
            if (typeToken.IsPrimitive)
                return Expression.Call(RecordReader, typeToken.MakeGenericMethod(AlignedSequentialRecordReader.Methods.Read), DataStream);

            if (typeToken == typeof(string))
                return Expression.Call(RecordReader, AlignedSequentialRecordReader.Methods.ReadString, DataStream);

            return null;
        }

        protected override int GetCardinality(MemberToken memberToken) => memberToken.Cardinality;

        public T Deserialize(Stream dataStream, in AlignedSequentialRecordReader recordReader)
        {
            Method.Invoke(dataStream, in recordReader, out var instance);
            return instance;
        }
    }
}
