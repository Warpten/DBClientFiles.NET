using System.IO;
using System.Linq.Expressions;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization.Generators;
using DBClientFiles.NET.Parsing.Shared.Records;

namespace DBClientFiles.NET.Parsing.Versions.WDBC
{
    internal sealed class SerializerGenerator<T> : TypedSerializerGenerator<T, SerializerGenerator<T>.MethodType>
    {
        public delegate void MethodType(Stream dataStream, ISequentialRecordReader recordReader, out T instance);

        private ParameterExpression DataStream { get; } = Expression.Parameter(typeof(Stream), "dataStream");

        private ParameterExpression RecordReader { get; } = Expression.Parameter(typeof(ISequentialRecordReader), "recordReader");

        protected override ParameterExpression ProducedInstance { get; } = Expression.Parameter(typeof(T).MakeByRefType(), "instance");

        public SerializerGenerator(TypeToken root, TypeTokenType memberType) : base(root, memberType)
        {
        }

        protected override Expression<MethodType> MakeLambda(Expression body)
        {
            return Expression.Lambda<MethodType>(body, new[] {
                DataStream,
                RecordReader,
                ProducedInstance
            });
        }

        public override Expression GenerateExpressionReader(TypeToken typeToken, MemberToken memberToken)
        {
            if (typeToken.IsPrimitive)
                return Expression.Call(RecordReader, typeToken.MakeGenericMethod(_ISequentialRecordReader.Read), DataStream);

            if (typeToken == typeof(string))
                return Expression.Call(RecordReader, _ISequentialRecordReader.ReadString, DataStream);

            return null;
        }

        protected override Expression MakePrologue() => null;
    }
}
