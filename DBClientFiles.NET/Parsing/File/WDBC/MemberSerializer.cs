using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    /// <summary>
    /// The member serializer for WDBC files structure members.
    /// </summary>
    internal sealed class MemberSerializer : BaseMemberSerializer
    {
        public MemberSerializer(ITypeMember memberInfo) : base(memberInfo)
        {
        }

        public override void VisitArrayNode(ref ExtendedMemberExpression memberAccess, Expression recordReader)
        {
            var elementType = memberAccess.MemberInfo.Type.GetElementType();
            if (elementType.IsPrimitive)
            {
                Produce(Expression.Assign(
                    memberAccess.Expression,
                    Expression.Call(recordReader,
                        _IRecordReader.ReadArray.MakeGenericMethod(elementType),
                        Expression.Constant(memberAccess.MemberInfo.Cardinality))));
            }
            else if (elementType == typeof(string))
            {
                Produce(Expression.Assign(
                    memberAccess.Expression,
                    Expression.Call(recordReader,
                        _IRecordReader.ReadStringArray,
                        Expression.Constant(memberAccess.MemberInfo.Cardinality))));
            }
            else
            {
                Produce(Expression.Assign(
                    memberAccess.Expression,
                    Expression.NewArrayBounds(elementType, Expression.Constant(memberAccess.MemberInfo.Cardinality))));
            }
        }

        public override void VisitNode(ref ExtendedMemberExpression memberAccess, Expression recordReader)
        {
            if (memberAccess.MemberInfo.Type.IsPrimitive)
            {
                Produce(Expression.Assign(
                    memberAccess.Expression,
                    Expression.Call(recordReader, _IRecordReader.Read.MakeGenericMethod(memberAccess.MemberInfo.Type))));
            }
            else if (memberAccess.MemberInfo.Type == typeof(string))
            {
                Produce(Expression.Assign(
                    memberAccess.Expression,
                    Expression.Call(recordReader, _IRecordReader.ReadString)));
            }
        }
    }
}
