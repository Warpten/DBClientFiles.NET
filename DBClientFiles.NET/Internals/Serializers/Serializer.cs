using DBClientFiles.NET.Internals.Versions;
using DBClientFiles.NET.IO;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Internals.Serializers
{
    internal class Serializer<TValue> where TValue : class, new()
    {
        private BaseFileReader<TValue> _fileReader;

        #region Life and death
        internal Serializer(BaseFileReader<TValue> fileReader)
        {
            _fileReader = fileReader;
        }
        #endregion

        private List<Expression> GetBasicReader(ExtendedMemberInfo memberInfo, params Expression[] arguments)
        {
            var expressionBucket = new List<Expression>();

            switch (memberInfo.CompressionType)
            {
                case MemberCompressionType.None:
                    if (!GetStreamDataReader(expressionBucket, memberInfo, arguments))
                        throw new InvalidOperationException();
                    break;
                // case MemberCompressionType.Bitpacked:
                //     expressionBucket.Add(GetBitpackedDataReader(memberInfo));
                //     break;
                // case MemberCompressionType.BitpackedPalletData:
                // case MemberCompressionType.BitpackedPalletArrayData:
                //     expressionBucket.Add(GetPalletDataReader(memberInfo));
                //     break;
                // case MemberCompressionType.CommonData:
                //     expressionBucket.Add(GetCommonDataReader(memberInfo));
                //     break;
                default:
                    throw new NotImplementedException();
            }

            return expressionBucket;
        }

        private bool GetStreamDataReader(List<Expression> body, ExtendedMemberInfo memberInfo, params Expression[] arguments)
        {
            var memberType = memberInfo.Type;
            if (memberType.IsArray)
                memberType = memberType.GetElementType();

            var memberTypeCode = Type.GetTypeCode(memberType);
            var podReader = GetBasicPODReader(readerInstance: arguments[0], memberTypeCode);

            var memberAccessExpr = memberInfo.MakeMemberAccess(arguments[0]);

            if (memberInfo.Type.IsArray)
            {
                body.Add(Expression.Assign(memberAccessExpr.Expression, Expression.NewArrayBounds(memberType, Expression.Constant(memberInfo.ArraySize))));

                for (var i = 0; i < memberInfo.ArraySize; ++i)
                {
                    var itemAccessExpr = Expression.ArrayIndex(memberAccessExpr.Expression, Expression.Constant(i));
                    body.Add(Expression.Assign(itemAccessExpr, podReader));
                }
            }

            return true;
        }

        private static List<Expression> GetInstanciationExpression(Expression objectInstance,Expression readerInstance, ExtendedMemberInfo memberInfo)
        {
            return null;
        }

        private static Expression GetBasicPODReader(Expression readerInstance, TypeCode valueTypeCode)
        {
            switch (valueTypeCode)
            {
                case TypeCode.Object:
                    break;

                case TypeCode.SByte:  return Expression.Call(readerInstance, _FileReader.ReadSByte);
                case TypeCode.UInt16: return Expression.Call(readerInstance, _FileReader.ReadUInt16);
                case TypeCode.UInt32: return Expression.Call(readerInstance, _FileReader.ReadUInt32);
                case TypeCode.UInt64: return Expression.Call(readerInstance, _FileReader.ReadUInt64);

                case TypeCode.Byte:   return Expression.Call(readerInstance, _FileReader.ReadByte);
                case TypeCode.Int16:  return Expression.Call(readerInstance, _FileReader.ReadInt16);
                case TypeCode.Int32:  return Expression.Call(readerInstance, _FileReader.ReadInt32);
                case TypeCode.Int64:  return Expression.Call(readerInstance, _FileReader.ReadInt64);

                case TypeCode.Single: return Expression.Call(readerInstance, _FileReader.ReadSingle);

                case TypeCode.String: return Expression.Call(readerInstance, _FileReader.ReadString);
            }
            throw new NotImplementedException();
        }
    }
}
