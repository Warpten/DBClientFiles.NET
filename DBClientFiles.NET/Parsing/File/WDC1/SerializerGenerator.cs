using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization.Generators;
using System;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Parsing.File.WDC1
{
    internal sealed class SerializerGenerator<T> : TypedSerializerGenerator<T>
    {
        private FieldInfoHandler<MemberMetadata> FieldInfoBlock { get; }

        public SerializerGenerator(IBinaryStorageFile storage, FieldInfoHandler<MemberMetadata> fieldInfoBlock) : base(storage.Type, storage.Options.TokenType)
        {
            FieldInfoBlock = fieldInfoBlock;

            Parameters.Add(Expression.Parameter(typeof(IRecordReader)));
            Parameters.Add(Expression.Parameter(typeof(T).MakeByRefType()));
        }

        public override Expression GenerateExpressionReader(TypeToken typeToken, MemberToken memberToken)
        {
            if (typeToken.IsPrimitive)
            {
                var methodCall = typeToken.MakeGenericMethod(_IRecordReader.Read);
                return Expression.Call(RecordReader, methodCall);
            }
            else if (typeToken == typeof(string))
                return Expression.Call(RecordReader, _IRecordReader.ReadString);

            return null;
        }

        protected override Expression FileParser => throw new NotImplementedException();
        protected override Expression RecordReader => Parameters[0];
        protected override Expression Instance => Parameters[1];
    }
}
