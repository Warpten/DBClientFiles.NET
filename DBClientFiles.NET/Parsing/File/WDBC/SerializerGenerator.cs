using System;
using System.Linq.Expressions;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization.Generators;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    internal sealed class SerializerGenerator<T, TMethod> : TypedSerializerGenerator<T, TMethod> where TMethod : Delegate
    {
        public SerializerGenerator(TypeToken root, TypeTokenType memberType) : base(root, memberType)
        {
        }

        public override Expression GenerateExpressionReader(TypeToken typeToken, MemberToken memberToken)
        {
            if (typeToken.Type.IsPrimitive)
            {
                var methodCall = _IRecordReader.Read.MakeGenericMethod(typeToken.Type);
                return Expression.Call(RecordReader, methodCall);
            }
            else if (typeToken.Type == typeof(string))
            {
                return Expression.Call(RecordReader, _IRecordReader.ReadString);
            }

            return null;
        }
    }
}
