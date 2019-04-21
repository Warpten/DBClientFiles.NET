using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Parsing.File.WDC1
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        public Serializer() : base()
        {

        }

        public override Expression VisitNode(Expression memberAccess, MemberToken memberInfo, ref DeserializerParameters parameters)
        {
            if (memberInfo.TypeToken.Type.IsArray)
            {
                var elementType = memberInfo.TypeToken.Type.GetElementType();
                if (elementType.IsPrimitive)
                {
                    // = ReadArray<T>(...);
                    return Expression.Call(parameters.Reader,
                        _IRecordReader.ReadArray.MakeGenericMethod(elementType),
                        Expression.Constant(memberInfo.Cardinality));
                }
                else if (elementType == typeof(string))
                {
                    // = ReadStringArray(...)
                    return Expression.Call(parameters.Reader,
                        _IRecordReader.ReadStringArray,
                        Expression.Constant(memberInfo.Cardinality));
                }

                return null;
            }

            if (memberInfo.TypeToken.Type.IsPrimitive)
            {
                // = Read<T>();
                return Expression.Call(parameters.Reader, _IRecordReader.Read.MakeGenericMethod(memberInfo.TypeToken.Type));
            }
            else if (memberInfo.TypeToken.Type == typeof(string))
            {
                // = ReadString();
                return Expression.Call(parameters.Reader, _IRecordReader.ReadString);
            }

            return null;
        }
    }
}
