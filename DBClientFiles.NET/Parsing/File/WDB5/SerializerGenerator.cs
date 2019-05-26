using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DBClientFiles.NET.Parsing.Enums;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization.Generators;

namespace DBClientFiles.NET.Parsing.File.WDB5
{
    internal sealed class SerializerGenerator<T, TMethod> : TypedSerializerGenerator<T, TMethod> where TMethod : Delegate
    {
        private IList<MemberMetadata> _memberMetadata;
        private int _memberIndex;

        public SerializerGenerator(TypeToken root, TypeTokenType memberType, IList<MemberMetadata> memberMetadata) : base(root, memberType)
        {
            _memberMetadata = memberMetadata;
        }

        public override Expression GenerateExpressionReader(TypeToken typeToken, MemberToken memberToken)
        {
            var memberMetadata = _memberMetadata[_memberIndex++];
            switch (memberMetadata.CompressionData.Type)
            {
                // We have to use immediate readers because all the other ones assume sequential reads
                case MemberCompressionType.None:
                case MemberCompressionType.Immediate:
                    if (typeToken.IsPrimitive)
                    {
                        return Expression.Call(RecordReader,
                            typeToken.MakeGenericMethod(_IRecordReader.ReadImmediate),
                            Expression.Constant(memberMetadata.CompressionData.Offset),
                            Expression.Constant(memberMetadata.CompressionData.Size));
                    }
                    else if (typeToken == typeof(string))
                        return Expression.Call(RecordReader,
                            _IRecordReader.ReadStringImmediate,
                            Expression.Constant(memberMetadata.CompressionData.Offset),
                            Expression.Constant(memberMetadata.CompressionData.Size));

                    break;
            }
            
            throw new InvalidOperationException("Unsupported compression type");
        }
    }
}
